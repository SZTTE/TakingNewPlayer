using UnityEngine;
using Assets.Script;
namespace Assets.Script
{
    public class Factory
    {
        private static GameObject _node;
        private static GameObject _link;

        public static void Init()
        {
            _node = Resources.Load<GameObject>("Prefab/Node");
            _link = Resources.Load<GameObject>("Prefab/Link");
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
    }
}
