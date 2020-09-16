using System.Collections.Generic;
using Assets.Script.Rocket;
using Unity.DocZh.Components;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Assets.Script
{
    public class UIManager : MonoBehaviour
    {
        public enum StateEnum
        {
            AllDisable,
            AllAble,
            OnGaming,
        }
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _speedUpButton;
        [SerializeField] private Button _speedDownButton;
        [SerializeField] private Button _drillRocketButton;
        [SerializeField] private Button _returnRocketButton;
        [SerializeField] private Button _exitButton;
        private List<Button> _buttonList;
        private static UIManager Instance;
        public static List<Button> ButtonList {
            get
            {
                if(Instance._buttonList==null)
                    Instance._buttonList = new List<Button>
                    {
                        Instance._startButton,
                        Instance._speedUpButton,
                        Instance._speedDownButton,
                        Instance._drillRocketButton,
                        Instance._returnRocketButton,
                        Instance._exitButton,
                    };
                return Instance._buttonList;
            }
        }
        public static StateEnum State
        {
            set
            {
                switch (value)
                {
                    case StateEnum.AllAble:
                        foreach (var b in ButtonList)
                        {
                            b.interactable = true;
                        }
                        break;
                    
                    case StateEnum.AllDisable:
                        foreach (var b in ButtonList)
                        {
                            b.interactable = false;
                        }
                        break;
                    case StateEnum.OnGaming:
                        foreach (var b in ButtonList)
                        {
                            b.interactable = false;
                        }
                        Instance._speedUpButton.interactable = true;
                        Instance._speedDownButton.interactable = true;
                        Instance._startButton.interactable = true;
                        Instance._startButton.GetComponentInChildren<Text>().text = "↙";
                        break;
                }
                
            }
        }
        UIManager()
        {
            Instance = this;
        }

        void Start()
        {
            _drillRocketButton.onClick.AddListener(() =>
            {
                State = StateEnum.AllDisable;
                var r = Factory.CreatDrillRocket();
                r.SetByMouse();
            });
            _returnRocketButton.onClick.AddListener(() =>
            {
                State = StateEnum.AllDisable;
                var r = Factory.CreatReturnRocket();
                r.SetByMouse();
            });
            _startButton.onClick.AddListener(() =>
            {
                State = StateEnum.OnGaming;
                Time.timeScale = 1f;
                foreach (var r in GameManager.RocketList)
                {
                    r.StateMachine.State = RocketBase.StateEnum.ReadyToLaunch;
                }
            });
        }

    }
}
