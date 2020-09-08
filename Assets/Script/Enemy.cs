using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Script
{
    public class Enemy : MonoBehaviour
    {
        private GameObject _smallPic;
        private GameObject _bigPic;
        private RoutePosition _routePosition;
        private float _moveDistance = 0.01f;
        public static float BigSize { get; } = 0.43f;
        public static float SmallSize { get; } = 0.25f;

        private GameObject SmallPic 
        {
            get
            {
                if(_smallPic==null) Init();
                return _smallPic;
            }
        }
        public GameObject BigPic
        {
            get
            {
                if(_bigPic==null) Init();
                return _bigPic;
            }
        }

        public RoutePosition Position
        {
            get { return _routePosition;}
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
                transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90,Vector3.forward);
            }
        }
        public float Size { get; private set; } = 0.68f;

        public float DistanceToNode(Node n)
        {
            if (Position.To == n) return Position.Distance;
            else return Position.Link.Distance - Position.Distance;
        }

        #region 给Move函数用的
        /// <summary>
        /// 根据当前位置，获得应该前往的下一个连接
        /// </summary>
        public Link NextLink
        {
            get
            {
                //1.获取所有的另一端点
                var links = GameManager.SearchLinks(Position.To);
                var linkDic = new Dictionary<float,Link>();//根据角度排序的linkList
                foreach (var link in links)//把每个节点都添加到linkDic里
                {
                    Node centerNode = Position.To;
                    Node anotherNode;
                    anotherNode = link.EndPoint1 == centerNode ? link.EndPoint2 : link.EndPoint1;
                    Vector2 delta = anotherNode.Position - centerNode.Position;
                    float angle = Mathf.Atan2(delta.y, delta.x);
                    linkDic.Add(angle,link);
                }
                //获取已经排好序的linkDic，寻找靠右的节点（编号比原来的连接大一，如果不存在则取编号最小的）
                var orderedLinks = linkDic.Values.ToList();
                int myLinkIndex = orderedLinks.FindIndex(x=>x==Position.Link);
                if (orderedLinks.Count - 1 == myLinkIndex) //我的链接是最后一个连接
                    return orderedLinks[0];
                else
                    return orderedLinks[myLinkIndex + 1];
            }
        }
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
                    Destroy(gameObject);
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

        /// <summary>
        /// 搜索所有挤压着我的人
        /// </summary>
        /// <param name="side">在我的那一侧？选填我所在连接的两个节点之一</param>
        public List<Enemy> SearchAllTowardMeCrowding(Node side)
        {
            if (side != Position.Link.EndPoint1 && side != Position.Link.EndPoint2)
            {
                Time.timeScale = 0;
                Debug.LogError("一个敌人尝试搜索临近人时，参数不是他两侧的节点");
            }
            
            //获取连接上所有的敌人，按照距离side的距离排序
            var enemiesOnLink = new List<Enemy>();
            foreach (var e in GameManager.EnemiesList)
            {
                if(e.Position.Link==Position.Link)
                    enemiesOnLink.Add(e);
            }
            enemiesOnLink.Sort((x,y)=>x.DistanceToNode(side)>y.DistanceToNode(side)?-1:1);
            
            //如果我是最靠近side的，就进行脱离人查找，反之就判断比我更靠近的那个人是否符合条件，进行递归查找
            int myIndex = enemiesOnLink.FindIndex(x => x == this);
            if (myIndex == 0)
            {//需要进行脱离人的链接查找
                Debug.LogError("还没有写的脱离人查找");
                return new List<Enemy>{this};
            }
            else
            {//查找比我更靠近side的那个人
                var target = enemiesOnLink[myIndex - 1];
                if (target.Position.To == side || DistanceToNode(side)-target.DistanceToNode(side)>Size+target.Size+0.5f)
                {//那个人不面向我，或者他离我太远。结束查找
                    return new List<Enemy>{this};
                }
                else
                {//那个人面向我并且距离合适，即我们呈挤压状态。对他进行递归查找
                    var preList = target.SearchAllTowardMeCrowding(side);
                    preList.Add(this);
                    return preList;
                }
            }
        }

        #endregion

        public void Move()
        {
            //情况检测，判断使用哪一种move函数
            foreach (var enemy in GameManager.EnemiesList)
            {//查找自己的对面每一个面向自己的敌人
                
            }
        }

        void Init()
        {
            _smallPic = transform.Find("EnemySmall").gameObject;
            _bigPic = transform.Find("EnemyBig").gameObject;
        }
        public void BecomeBig()
        {
            BigPic.SetActive(true);
            SmallPic.SetActive(false);
            Size = BigSize;
        }
        public void BecomeSmall()
        {
            BigPic.SetActive(false);
            SmallPic.SetActive(true);
            Size = SmallSize;
        }
    }
}
