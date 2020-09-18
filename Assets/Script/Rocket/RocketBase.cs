using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
namespace Assets.Script.Rocket
{
    public class RocketBase : MonoBehaviour
    {
        public enum StateEnum
        {
            None,
            SettingPosition,
            SettingOrientation,
            Sleep,
            ReadyToLaunch,
            Flying,
        }

        public StateMachine<StateEnum> StateMachine { get; set; } =new StateMachine<StateEnum>();
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
        public Vector2 Position {set=>transform.position = value; get=>transform.position;}

        protected virtual void OnMouseDown()
        {
            if (StateMachine.State != StateEnum.ReadyToLaunch) return;
            StateMachine.State = StateEnum.Flying;
            DottedLineVisible = false;
        }
        protected void OnMouseEnter()
        {
            if (StateMachine.State != StateEnum.ReadyToLaunch) return;
            DottedLineVisible = true;
        }

        protected void OnMouseExit()
        {
            if (StateMachine.State != StateEnum.ReadyToLaunch) return;
            DottedLineVisible = false;
        }

        protected RocketBase()
        {
            GameManager.RocketList.Add(this);
            //初始化状态机
            StateMachine.RegisterAction(StateEnum.SettingPosition, () =>
            {
                Position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Input.GetMouseButtonDown(0))
                {
                    DottedLineVisible = true;
                    StateMachine.State = StateEnum.SettingOrientation;
                }
            });
            StateMachine.RegisterAction(StateEnum.SettingOrientation, () =>
            {
                Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Orientation = (mouse - Position).normalized;
                if (Input.GetMouseButtonDown(0))
                {
                    UIManager.State = UIManager.StateEnum.AllAble;
                    DottedLineVisible = false;
                    StateMachine.State = StateEnum.Sleep;
                }
            });
            StateMachine.RegisterAction(StateEnum.Flying, () =>
            {
                var orientation = transform.up;
                transform.position += orientation * Speed*UIManager.CustomTimeScale;
            });
        }

        void FixedUpdate()
        {
        }

        void Update()
        {
            StateMachine.Run();
            Debug.Log("My State is"+StateMachine.State);
        }
        public void SetByMouse()
        {
            StateMachine.State = StateEnum.SettingPosition;
        }
    }
}
