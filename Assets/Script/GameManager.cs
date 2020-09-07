using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    public class GameManager : MonoBehaviour
    {
        public static List<Link> LinkList { get; private set; }
        private Enemy _e;
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
            
            RoutePosition r = new RoutePosition(LinkList[0],node0,0);
            _e = Factory.CreatEnemy(r);
        }

        // Update is called once per frame
        void Update()
        {
            _e.MoveForward();
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
    }
}
