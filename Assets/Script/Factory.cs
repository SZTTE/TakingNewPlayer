using UnityEngine;
using Assets.Script;
namespace Assets.Script
{
    public class Factory
    {
        private static GameObject _node;

        public static void Init()
        {
            _node = Resources.Load<GameObject>("Prefab/Node");
            Debug.Log(_node);
        }

        public static Node CreatNode(Vector2 position)
        {
            if(_node == null) Init();
            GameObject node = GameObject.Instantiate(_node);
            node.transform.position = position;
            return node.GetComponent<Node>();
        }
    }
}
