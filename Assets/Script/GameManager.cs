using System.Collections.Generic;
using System.Linq;
using Assets.Script.Rocket;
using TMPro;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Assets.Script
{
    public class GameManager : MonoBehaviour
    {
        public static List<Link> LinkList { get; private set; }
        public static int Frame { get; private set; } = 0;
        public static Dictionary<int, int> FrameAndTimes { get; }=new Dictionary<int, int>();

        void Start()
        {

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

            var rr = Factory.CreatReturnRocket(new Vector2(-3, -3), new Vector2(1, 1));
            
            RoutePosition r = new RoutePosition(LinkList[0],node0,0);
            EnemiesList.Add(Factory.CreatEnemy(r)); 
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Frame++;
            FrameAndTimes[Frame] = 0;
            Time.timeScale = 1f;
            if(test_shouldBorn)
                if (EnemiesList.last().Position.Distance>=EnemiesList.last().Size+Enemy.SmallSize)
                {
                    RoutePosition r = new RoutePosition(LinkList[0],LinkList[0].EndPoint2,0);
                    var e = Factory.CreatEnemy(r);
                    e.BecomeSmall();
                    EnemiesList.Add(e); 
                }
            LoopEnemyMove();
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

        #region 敌人的管理
        public static EnemyList EnemiesList { get; private set; } = new EnemyList();
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
