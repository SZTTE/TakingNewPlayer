using UnityEngine;
namespace Assets.Script.Rocket
{
    public class RocketBase : MonoBehaviour
    {
        private GameObject _dottedLine;
        public bool Launched { get; private set; } = false;
        public float Speed { get; } = 0.42f;
        private GameObject DottedLine
        {
            get
            {
                if (_dottedLine == null)
                    _dottedLine = transform.Find("DottedLine").gameObject;
                return _dottedLine;
            }
        }
        public Vector2 Orientation
        {
            get => transform.up;
            set
            {
                float angle = Mathf.Atan2(value.y, value.x);
                transform.rotation = Quaternion.AngleAxis(angle*Mathf.Rad2Deg-90,Vector3.forward);
                Debug.Log("Set");
            }
        }
        public bool DottedLineVisible {set=> DottedLine.SetActive(value);}
        public Vector2 Position {set=>transform.position = value; }

        protected virtual void OnMouseDown()
        {
            Launched = true;
        }
        void FixedUpdate()
        {
            if (Launched)
            {
                var orientation = transform.up;
                transform.position += orientation * Speed;
            }
        }
    }
}
