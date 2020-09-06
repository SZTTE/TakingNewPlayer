﻿using UnityEngine;

namespace Assets.Script
{
    public class GameManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Node node1 = Factory.CreatNode(new Vector2(1, 0));
            node1.BecomeBegin();
            Node node2 = Factory.CreatNode(new Vector2(0, 4));
            node2.BecomeEnd();
            Link link = Factory.CreatLink();
            link.EndPoint1 = node1;
            link.EndPoint2 = node2;
            
            var position = new RoutePosition(link,node2,2);
            var enemy = Factory.CreatEnemy(position);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
