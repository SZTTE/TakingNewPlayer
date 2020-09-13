using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script
{
    public class Node : MonoBehaviour
    {
        private GameObject _beginPic;
        private GameObject _endPic;
        private GameObject _normalPic;
        public Dictionary<float, Link> Links { get; } = new Dictionary<float, Link>();

        public GameObject BeginPic
        {
            get
            {
                if (_beginPic == null) Init();
                return _beginPic;
            }
        }
        public GameObject EndPic
        {
            get
            {
                if (_endPic == null) Init();
                return _endPic;
            }
        }
        public GameObject NormalPic
        {
            get
            {
                if (_normalPic == null) Init();
                return _normalPic;
            }
        }

        private void Init()
        {
            _beginPic = transform.Find("RelocateBegin").gameObject;
            _endPic = transform.Find("RelocateEnd").gameObject;
            _normalPic = transform.Find("RelocateNormal").gameObject;
        }

        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public void BecomeBegin()
        {
            BeginPic.SetActive(true);
            EndPic.SetActive(false);
            NormalPic.SetActive(false);
        }
        public void BecomeEnd()
        {
            BeginPic.SetActive(false);
            EndPic.SetActive(true);
            NormalPic.SetActive(false);
        }
        public void BecomeNormal()
        {
            BeginPic.SetActive(false);
            EndPic.SetActive(false);
            NormalPic.SetActive(true);
        }
        public void RegisterLink(Link l)
        {
            Node centerNode = this;
            Node anotherNode;
            anotherNode = l.EndPoint1 == centerNode ? l.EndPoint2 : l.EndPoint1;
            Vector2 delta = anotherNode.Position - centerNode.Position;
            float angle = Mathf.Atan2(delta.y, delta.x);
            Links.Add(angle,l);
        }

        public Link RightSideOf(Link l)
        {
            if (Links.Count <= 1)
            {
                Debug.LogError("别你妈搜了，没路了");
                return null;
            }
            var ordered = Links.Values.ToList();
            int lPosition = ordered.IndexOf(l);
            if (lPosition == Links.Count-1) return ordered[0];
            else return ordered[lPosition + 1];
        }
        public Link LeftSideOf(Link l)
        {
            if (Links.Count <= 1)
            {
                Debug.LogError("别你妈搜了，没路了");
                return null;
            }
            var ordered = Links.Values.ToList();
            int lPosition = ordered.IndexOf(l);
            if (lPosition == 0) return ordered.Last();
            else return ordered[lPosition - 1];
        }
    }
}
