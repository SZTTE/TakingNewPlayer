using UnityEngine;
namespace Assets.Script.Rocket
{
    public class RocketBase : MonoBehaviour
    {
        public bool Launched { get; private set; } = false;
        public float Speed { get; } = 0.42f;

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
