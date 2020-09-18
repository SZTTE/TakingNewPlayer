﻿using System.Collections.Generic;
using System.Linq;
using Assets.Script.Rocket;
using TMPro;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Assets.Script
{
    public class GameManager : MonoBehaviour
    {
        public enum StateEnum
        {
            None,
            SettingRocket,
            OnPlaying,
            GameFail,
            GameSuccess,
        }

        public static StateMachine<StateEnum> StateMachine { get; set; }
        public static List<Link> LinkList { get; private set; }
        public static List<RocketBase> RocketList { get; private set; }
        public static int Frame { get; private set; } = 0;
        public static int DrillRocketUnused { get; set; }
        public static int ReturnRocketUnused { get; set; }

        GameManager()
        {
            StateMachine = new StateMachine<StateEnum>();
            RocketList = new List<RocketBase>();
            EnemiesList = new EnemyList();
        }

        void Start()
        {
            DrillRocketUnused = 5;
            ReturnRocketUnused = 2;
            StateMachineInit();
            Time.timeScale = 0f;
            Node begin = Factory.CreatNode(new Vector2(-6.44f, 2.91f));
            begin.BecomeBegin();
            Node end = Factory.CreatNode(new Vector2(4.43f,2.8f));
            end.BecomeEnd();
            Node node0 = Factory.CreatNode(new Vector2(-1.13f, 0.22f));
            Node node1 = Factory.CreatNode(new Vector2(3.83f, -3.29f));
            Node node2 = Factory.CreatNode(new Vector2(4, 0));
            
            LinkList = new List<Link>
            {
                Factory.CreatLink(begin,node0),
                Factory.CreatLink(node0,node1),
                Factory.CreatLink(node0,node2),
                Factory.CreatLink(node1,node2),
                Factory.CreatLink(node0,end)
            };

            RoutePosition r = new RoutePosition(LinkList[0],node0,0);
            EnemiesList.Add(Factory.CreatEnemy(r)); 
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            StateMachine.Run();
        }

        private bool test_shouldBorn = true;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var link = EnemiesList[3].Position.Link;
                var result =EnemiesList[3].SearchOneCrowding(link.GetNodeBeside(EnemiesList[3].Position.To));
                foreach (var e in result)
                {
                    if (e == null) continue;
                    Debug.Log("按下空格來使某些敵人變大");
                    e.BecomeBig();
                    Time.timeScale = 0;
                }
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                test_shouldBorn = !test_shouldBorn;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                EnemiesList[0].TurnAround();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                EnemiesList[1].TurnAround();
            }
        }

        public static List<Link> SearchLinks(Node node)
        {
            var result = new List<Link>();
            foreach (var link in LinkList)
            {
                if(link.EndPoint1==node||link.EndPoint2==node)
                    result.Add(link);
            }

            return result;
        }

        private void StateMachineInit()
        {
            StateMachine.State = StateEnum.SettingRocket;
            StateMachine.RegisterAction(StateEnum.OnPlaying, () =>
            {
                Frame++;
                if(test_shouldBorn)
                    if (EnemiesList.last().Position.Distance>=EnemiesList.last().Size+Enemy.SmallSize)
                    {
                        RoutePosition r = new RoutePosition(LinkList[0],LinkList[0].EndPoint2,0);
                        var e = Factory.CreatEnemy(r);
                        e.BecomeSmall();
                        EnemiesList.Add(e); 
                    }
                Time.timeScale = UIManager.CustomTimeScale;
                LoopEnemyMove();
                if (EnemiesList.isEmpty())
                {
                    StateMachine.State = StateEnum.GameSuccess;
                    UIManager.Instruction = "游戏胜利:D";
                }
            });
        }

        public static void GameStart()
        {
            StateMachine.State = StateEnum.OnPlaying;
            foreach (var r in GameManager.RocketList)
            {
                r.StateMachine.State = RocketBase.StateEnum.ReadyToLaunch;
            }
            Time.timeScale = UIManager.CustomTimeScale;
        }

        public static void GameFail()
        {
            if (StateMachine.State != StateEnum.OnPlaying)
            {
                Debug.LogError("游戏都没在玩，是谁调用这个函数啊？？");
                return;
            }
            Time.timeScale = 0;
            StateMachine.State = StateEnum.GameFail;
            UIManager.Instruction = "游戏结束：敌人到达终点\n点右边的按钮重置关卡吧";
        }

        #region 敌人的管理
        public static EnemyList EnemiesList { get; private set; }
        public static EnemyList EnemiesToDelete { get; } = new EnemyList();

        /// <summary>
        /// 每帧运行，处理敌人的移动
        /// </summary>
        private void LoopEnemyMove()
        {
            //零、充值所有敌人的期望
            foreach (var e in EnemiesList)
            {
                e.ResetExpectation();
                //Debug.Log("Im the last, FC="+EnemiesList.Last().CrowdedFront+", BC="+EnemiesList.Last().CrowdedBack);
            }
            
            //一、设置所有敌人的前力后力
            foreach (var e in EnemiesList)
            {
                if ( (e.CrowdedFront && (!e.CrowdedBack))//这个人在末端
                    || e.HasConflict//或者这个人有冲突
                    )
                {//对这个人，向前开始运算它们的前进后退期望（递归）
                    e.SetAllExpectation();
                }
            }

            //二、让每个敌人根据自己的情况运动，应该被删掉的敌人会把自己放到EnemiesToDelete里
            for(int i=0;i<EnemiesList.Count;i++)
            {
                if (EnemiesList[i] == null) continue;
                EnemiesList[i].Move();
            }
            
            //三、删掉刚才应该被删掉的敌人
            foreach (var e in EnemiesToDelete)
            {
                if (EnemiesList.Contains(e))
                {
                    EnemiesList.Remove(e);
                    Destroy(e.gameObject);
                }
            }
        }

        #endregion
    }
}
