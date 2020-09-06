using UnityEngine;
using Assets.Script;
namespace Assets.Script
{
    public class Factory
    {
        private static GameObject _node;
        private static GameObject _link;
        private static GameObject _enemy;

        public static void Init()
        {
            _node = Resources.Load<GameObject>("Prefab/Node");
            _link = Resources.Load<GameObject>("Prefab/Link");
            _enemy = Resources.Load<GameObject>("Prefab/Enemy");
            Debug.Log(_node);
        }

        public static Node CreatNode(Vector2 position)
        {
            if(_node == null) Init();
            GameObject node = GameObject.Instantiate(_node);
            node.transform.position = position;
            return node.GetComponent<Node>();
        }

        public static Link CreatLink()
        {
            if (_link == null) Init();
            GameObject link = GameObject.Instantiate(_link);
            return link.GetComponent<Link>();
        }
        public static Link CreatLink(Node node1,Node node2)
        {
            Link l = CreatLink();
            l.EndPoint1 = node1;
            l.EndPoint2 = node2;
            return l;
        }

        public static Enemy CreatEnemy(RoutePosition routePosition)
        {
            if(_enemy == null) Init();
            GameObject enemy = GameObject.Instantiate(_enemy);
            Enemy script =  enemy.GetComponent<Enemy>();
            script.Position = routePosition;
            return script;
        }
    }
}
