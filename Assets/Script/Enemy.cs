﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Script
{
    public class Enemy : MonoBehaviour
    {
        public enum EnemyTypeEnum
        {
            None,
            Small,
            Big
        }

        public enum MoveEnum
        {
            None,
            Forward,
            Backward,
            Stop
        }

        private GameObject _smallPic;
        private GameObject _bigPic;
        private RoutePosition _routePosition;
        private float _moveDistance = 0.1f;
        private float _touchDistance = 0.5f; //两个人检测接触时，在他们之间隔着这个距离也算作接触
        private EnemyTypeEnum _type = EnemyTypeEnum.Big;
        public static float BigSize { get; } = 0.43f;
        public static float SmallSize { get; } = 0.25f;
        private GameObject SmallPic
        {
            get
            {
                if (_smallPic == null) Init();
                return _smallPic;
            }
        }
        public GameObject BigPic
        {
            get
            {
                if (_bigPic == null) Init();
                return _bigPic;
            }
        }
        public RoutePosition Position
        {
            get => _routePosition;
            set
            {
                //赋值
                _routePosition = value;

                //设置位置
                Vector2 to = _routePosition.To.Position;
                Vector2 from = Position.Link.GetNodeBeside(Position.To).Position;
                Vector2 delta = to - from;
                if (_routePosition.Distance > delta.magnitude) Debug.LogError("有一个敌人被设置了错误的位置：距连接开头的距离大于连接的距离");
                transform.position = from + delta.normalized * _routePosition.Distance;

                //设置朝向
                transform.rotation =
                    Quaternion.AngleAxis(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90, Vector3.forward);
            }
        }
        public Dictionary<int, MoveEnum> noDectectMovement { get; set; } = new Dictionary<int, MoveEnum>();//帧-》无检测行动
        public Vector2 GlobalPosition => transform.position;
        public float Size { get; private set; } = 0.68f;
        public int PushingPriority { get; private set; } = 2;
        public EnemyTypeEnum Type
        {
            get => _type;
            set
            {
                _type = value;
                switch (value)
                {
                    case EnemyTypeEnum.Big:
                        BigPic.SetActive(true);
                        SmallPic.SetActive(false);
                        Size = BigSize;
                        PushingPriority = 2;
                        break;
                    case EnemyTypeEnum.Small:
                        BigPic.SetActive(false);
                        SmallPic.SetActive(true);
                        Size = SmallSize;
                        PushingPriority = 1;
                        break;
                }
            }
        }

        public float DistanceToNode(Node n)
        {
            if (Position.To == n) return Position.Link.Distance - Position.Distance;
            else return Position.Distance;
        }
        /// <summary>
        /// 根据当前位置，获得应该前往的下一个连接
        /// </summary>
        public Link NextLink
        {
            get
            {
                return GetNextLink(Position);
            }
        }
        public static Link GetNextLink(RoutePosition r)
        {
            return r.To.RightSideOf(r.Link);
        }
        public static Link GetPreLink(RoutePosition r)
        {
            return r.From.LeftSideOf(r.Link);
        }
        public Link PreLink => GetPreLink(Position);
        public bool CrowdedFront
        {
            get
            {
                var result = SearchOneCrowding(Position.To);
                if (result[0] == null && result[1] == null && result[2] == null) return false;
                else return true;
            }
        }
        public bool CrowdedBack
        {
            get
            {
                var result = SearchOneCrowding(Position.From);
                if (result[0] == null && result[1] == null && result[2] == null) return false;
                else return true;
            }
        }

        #region 为Move服务
        private void MoveForward(float distance)
        {
            RoutePosition targetPosition = Position;
            targetPosition.Distance += distance;
            if (targetPosition.Distance >= Position.Link.Distance)//通过了节点
            {
                if (Position.To.EndPic.activeSelf)
                {//到达终点
                    Time.timeScale = 0;
                    Debug.Log("should stop");
                    Debug.Log("游戏结束：敌人到达终点");
                }
                else if (GameManager.SearchLinks(Position.To).Count == 1)
                {//到达末端
                    Die();
                }
                else
                {//通过一般末端
                    float distanceUsed = targetPosition.Distance - Position.Link.Distance;//在本次连接中移动的距离
                    //设置新出发地，并且以剩余距离为参数重新出发
                    targetPosition.Link = NextLink;
                    targetPosition.To = targetPosition.Link.GetNodeBeside(Position.To);
                    targetPosition.Distance = 0;
                    Position = targetPosition;
                    MoveForward(distance-distanceUsed);
                }
            }
            else
            {//没有通过节点
                Position = targetPosition;
            }
        }
        public void MoveForward()
        {
            MoveForward(_moveDistance);
        }
        public void TurnAround()
        {
            var p = Position;
            p.To = p.From;
            p.Distance = p.Link.Distance - p.Distance;
            Position = p;
        }
        private void MoveBack()
        {
            //掉头、向前走再掉头
            TurnAround();
            MoveForward(_moveDistance);
            TurnAround();
        }
        private void MoveStop()
        {
        }

        /// <summary>
        /// 0距离side最远,last距离side最近
        /// </summary>
        private static EnemyList GetOrderedEnemyBySide(Link link,Node side)
        {
            var enemiesOnLink = new EnemyList();
            foreach (var e in GameManager.EnemiesList)
            {
                if(e.Position.Link==link)
                    enemiesOnLink.Add(e);
            }
            enemiesOnLink.Sort((x,y)=>x.DistanceToNode(side)>y.DistanceToNode(side)?-1:1);
            return enemiesOnLink;
        }
        /// <summary>
        /// 对某条链接从某个端点开始，展开长度为Distance的搜索，获取全部挤压着的敌人
        /// </summary>
        /// <return>搜索到的敌人，最多只能搜索到一个</return>
        public static Enemy SearchOneCrowding(Link link, Node from, float distance)
        {//放弃搜索道路时搜索到两条下路的情况
            var enemiesOnLink = GetOrderedEnemyBySide(link, from);
            if (enemiesOnLink.Count == 0)
            {//路上没有敌人时，看这条路够不够长
                if (link.Distance > distance)
                {//长度足够，没有敌人就返回一个空list
                    return null;
                }
                else
                {//长度不足，需要对下一条路径搜索。但有一个特殊情况
                    var linksOfNextNode = GameManager.SearchLinks(link.GetNodeBeside(from));
                    if (linksOfNextNode.Count == 1) //特殊情况，下一条链接不存在
                        return null;

                    //正常情况：继续对下一条链接进行搜索
                    return SearchOneCrowding(GetPreLink(new RoutePosition(link, from, 0)), link.GetNodeBeside(from),
                        distance - link.Distance);
                }
            }
            else
            {//路上有敌人时，根据敌人类型判断是否接纳他
                var target = enemiesOnLink.Last();
                if (Math.Abs(target.Size - SmallSize) < 1e-5)
                {//如果目标是小的：我们要对distance进行进一步减小。因为我们当初在设置distance时默认这个目标是大的
                    distance -= BigSize - SmallSize;
                }
                //真正开始判断目标是否离from端点足够近
                if (target.DistanceToNode(from) <= distance && target.Position.To == from)
                {
                    var result = GetOrderedEnemyBySide(link, from).Last();
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 搜索所有挤压着我的人,注意这个Search会搜索到自己！！
        /// </summary>
        /// <param name="side">在我的那一侧？选填我所在连接的两个节点之一</param>
        /// <return>必定有三位，0左1中2右</return>
        public EnemyList SearchOneCrowding(Node side)
        {
            var result = new EnemyList() {null, null, null};
            if (side != Position.Link.EndPoint1 && side != Position.Link.EndPoint2)
            {
                Time.timeScale = 0;
                Debug.LogError("一个敌人尝试搜索临近人时，参数不是他两侧的节点");
            }
            
            //获取连接上所有的敌人，按照距离side的距离排序
            var enemiesOnLink = GetOrderedEnemyBySide(Position.Link,side);
            enemiesOnLink.Reverse();
            //如果我是最靠近side的，就进行脱离人查找，反之就判断比我更靠近的那个人是否符合条件
            int myIndex = enemiesOnLink.FindIndex(x => x == this);
            if (myIndex == 0)
            {//需要进行脱离人的链接查找，这里的算法还要实现搜索哪条路（如果那是一个末端节点，就截止搜索）
                //检查来源是否是末端
                var linkList = GameManager.SearchLinks(Position.From);
                if(linkList.Count==1) return new EnemyList{this};
                
                //展开脱离人的查找
                var myPos = Position;
                myPos.To = myPos.Link.GetNodeBeside(side);
                var targetLink = side.LeftSideOf(Position.Link);
                var left = SearchOneCrowding(targetLink,side,Size+BigSize-DistanceToNode(side)+_touchDistance);
                targetLink = side.RightSideOf(Position.Link);
                var right = SearchOneCrowding(targetLink, side,
                    Size + BigSize - DistanceToNode(side) + _touchDistance);
                result[0] = left;
                result[2] = right;
                return result;
            }
            else
            {//查找比我更靠近side的那个人
                var target = enemiesOnLink[myIndex - 1];
                if (target.Position.To == side || DistanceToNode(side)-target.DistanceToNode(side)>Size+target.Size+_touchDistance)
                {//那个人不面向我，或者他离我太远。结束查找
                    return result;
                }
                else
                {//那个人面向我并且距离合适，即我们呈挤压状态。对他进行递归查找
                    result[1] = target;
                    return result;
                }
            }
        }
        /*
        /// <summary>
        /// 如果有注定的行动方式，就直接行动
        /// </summary>
        public bool TryNoDetectMove()
        {
            if (noDectectMovement.ContainsKey(GameManager.Frame) == false) return false;
            switch (noDectectMovement[GameManager.Frame])
            {
                case MoveEnum.Backward:
                    MoveBack();
                    break;
                case MoveEnum.Forward:
                    MoveForward();
                    break;
                case MoveEnum.Stop:
                    MoveStop();
                    break;
            }

            return true;

        }*/

        #endregion
        /*public void Move()
        {//对每个敌人，都查找自己的对面每一个面向自己的敌人
            if (TryNoDetectMove()) return;//要是有注定的移动方式，就直接退出
            //获取自己前面、自己后面（包括自己）的最大优先级
            var enemiesAheadTowardMe = SearchAllCrowding(Position.To);
            enemiesAheadTowardMe.Remove(this);
            var highestAhead = enemiesAheadTowardMe.isEmpty()?0:enemiesAheadTowardMe.Biggest.PushingPriority;
            var enemiesBehindTowardMe = SearchAllCrowding(Position.From);
            var highestBehind = enemiesBehindTowardMe.isEmpty()?0:enemiesBehindTowardMe.Biggest.PushingPriority;
            if (highestAhead > highestBehind)
            {//我应该向后
                MoveBack();
                foreach (var e in enemiesAheadTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Forward);
                foreach (var e in enemiesBehindTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Backward);
            }
            else if (highestAhead==highestBehind)
            {//我应该停止
                MoveStop();
                foreach (var e in enemiesAheadTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Stop);
                foreach (var e in enemiesBehindTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Stop);
            }
            else
            {//我应该前进
                MoveForward();
                Debug.Log("Im at"+GameManager.EnemiesList.FindIndex(x=>x==this)+","+enemiesBehindTowardMe.Count);
                foreach (var e in enemiesBehindTowardMe)
                {
                    Debug.Log("inlist="+GameManager.EnemiesList.IndexOf(e));
                }
                foreach (var e in enemiesAheadTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Backward);
                foreach (var e in enemiesBehindTowardMe) e.noDectectMovement.Add(GameManager.Frame,MoveEnum.Forward);
            }
        }*/

        void Die()
        {
            GameManager.EnemiesList.Remove(this);
            Destroy(gameObject);
        }

        void Init()
        {
            _smallPic = transform.Find("EnemySmall").gameObject;
            _bigPic = transform.Find("EnemyBig").gameObject;
        }
        public void BecomeBig()
        {
            Type = EnemyTypeEnum.Big;
        }
        public void BecomeSmall()
        {
            Type = EnemyTypeEnum.Small;
        }
    }
}
