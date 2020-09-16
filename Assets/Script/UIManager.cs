using System.Collections.Generic;
using Unity.DocZh.Components;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Assets.Script
{
    public class UIManager : MonoBehaviour
    {
        public enum StateEnum
        {
            AllDisable,
            AllAble,
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
                }
                
            }
        }
        UIManager()
        {
            Instance = this;
        }

        
        
    }
}
