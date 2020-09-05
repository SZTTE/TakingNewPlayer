using UnityEngine;

namespace Assets.Script
{
    public class GameManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Node node = Factory.CreatNode(new Vector2(0, 0));
            node.BecomeBegin();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
