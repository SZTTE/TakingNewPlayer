using System;
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
        private GameObject _smallPic;
        private GameObject _bigPic;
        private RoutePosition _routePosition;
        private float _moveDistance = 0.01f;
        private float _touchDistance = 0.1f;//两个人检测接触时，在他们之间隔着这个距离也算作接触
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

        public Vector2 GlobalPosition { get => transform.position; }
        public float Size { get; private set; } = 0.68f;
        public float DistanceToNode(Node n)
        {
            if (Position.To == n) return Position.Link.Distance - Position.Distance;
            else return Position.Distance;
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
        public Link PreLink
        {
            get
            {
                return GetPreLink(Position);
            }
        }
        public static Link GetPreLink(RoutePosition r)
        {
            //1.获取所有的另一端点
            var links = GameManager.SearchLinks(r.Link.GetNodeBeside(r.To));
            var linkDic = new Dictionary<float,Link>();//根据角度排序的linkList
            foreach (var link in links)//把每个节点都添加到linkDic里
            {
                Node centerNode = r.Link.GetNodeBeside(r.To);
                Node anotherNode;
                anotherNode = link.EndPoint1 == centerNode ? link.EndPoint2 : link.EndPoint1;
                Vector2 delta = anotherNode.Position - centerNode.Position;
                float angle = Mathf.Atan2(delta.y, delta.x);
                linkDic.Add(angle,link);
            }
            //获取已经排好序的linkDic，寻找靠右的节点（编号比原来的连接大一，如果不存在则取编号最小的）
            var orderedLinks = linkDic.Values.ToList();
            int myLinkIndex = orderedLinks.FindIndex(x=>x==r.Link);
            if (myLinkIndex==0) //我的链接是第一个连接
                return orderedLinks.Last();
            else
                return orderedLinks[myLinkIndex - 1];
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
        /// 0距离side最远,last距离side最近
        /// </summary>
        private static List<Enemy> GetOrderedEnemyBySide(Link link,Node side)
        {
            var enemiesOnLink = new List<Enemy>();
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
        public static List<Enemy> SearchAllCrowding(Link link, Node from, float distance)
        {
            var enemiesOnLink = GetOrderedEnemyBySide(link, from);
            if (enemiesOnLink.Count == 0)
            {//路上没有敌人时，看这条路够不够长
                Debug.Log("这条路上没有敌人");
                if (link.Distance > distance)
                {//长度足够，没有敌人就返回一个空list
                    return new List<Enemy>();
                }
                else
                {//长度不足，需要对下一条路径搜索。但有一个特殊情况
                    var linksOfNextNode = GameManager.SearchLinks(link.GetNodeBeside(from));
                    if(linksOfNextNode.Count==1)//特殊情况，下一条链接不存在
                        return new List<Enemy>();

                    //正常情况：继续对下一条链接进行搜索
                    return SearchAllCrowding(GetPreLink(new RoutePosition(link, from, 0)), link.GetNodeBeside(from),
                        distance - link.Distance);
                }
            }
            else
            {//路上有敌人时，根据敌人类型判断是否接纳他
                var target = enemiesOnLink.Last();
                if (target.Size == SmallSize)
                {//如果目标是小的：我们要对distance进行进一步减小。因为我们当初在设置distance时默认这个目标是大的
                    distance -= BigSize - SmallSize;
                }
                Debug.Log("between"+from.Position+"and"+target.GlobalPosition+"is"+target.DistanceToNode(from)+",And Search Distance is "+distance);//from是正确的，target是错误的，distance是正确的
                //真正开始判断目标是否离from端点足够近
                if (target.DistanceToNode(from) <= distance)
                {
                    var preList = target.SearchAllCrowding(link.GetNodeBeside(from));
                    preList.Add(target);
                    return preList;
                }
                else
                {
                    return new List<Enemy>();
                }
            }
        }
        /// <summary>
        /// 搜索所有挤压着我的人
        /// </summary>
        /// <param name="side">在我的那一侧？选填我所在连接的两个节点之一</param>
        public List<Enemy> SearchAllCrowding(Node side)
        {
            if (side != Position.Link.EndPoint1 && side != Position.Link.EndPoint2)
            {
                Time.timeScale = 0;
                Debug.LogError("一个敌人尝试搜索临近人时，参数不是他两侧的节点");
            }
            
            //获取连接上所有的敌人，按照距离side的距离排序
            var enemiesOnLink = GetOrderedEnemyBySide(Position.Link,side);
            enemiesOnLink.Reverse();
            //如果我是最靠近side的，就进行脱离人查找，反之就判断比我更靠近的那个人是否符合条件，进行递归查找
            int myIndex = enemiesOnLink.FindIndex(x => x == this);
            if (myIndex == 0)
            {//需要进行脱离人的链接查找，这里的算法还要实现搜索哪条路（如果那是一个末端节点，就截止搜索）
                //检查来源是否是末端
                var linkList = GameManager.SearchLinks(Position.From);
                if(linkList.Count==1) return new List<Enemy>{this};
                
                //展开脱离人的查找
                var targetLink = PreLink;
                var preList = SearchAllCrowding(targetLink,Position.From,Size+BigSize-DistanceToNode(Position.From)+_touchDistance);
                preList.Add(this);
                return preList;
            }
            else
            {//查找比我更靠近side的那个人
                var target = enemiesOnLink[myIndex - 1];
                if (target.Position.To == side || DistanceToNode(side)-target.DistanceToNode(side)>Size+target.Size+_touchDistance)
                {//那个人不面向我，或者他离我太远。结束查找
                    return new List<Enemy>{this};
                }
                else
                {//那个人面向我并且距离合适，即我们呈挤压状态。对他进行递归查找
                    var preList = target.SearchAllCrowding(side);
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
