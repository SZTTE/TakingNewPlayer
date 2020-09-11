using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Assets.Script
{
    public class EnemyList : List<Enemy>
    {
        public EnemyList(int n):base(n)
        {
        }
        public EnemyList():base()
        {
            
        }

        public Enemy Biggest {
            get
            {
                if(this.isEmpty()) Debug.LogError("不应该对一个空EnemyList调用Biggest");
                Enemy result = this[0];
                foreach (var e in this)
                {
                    if (e.PushingPriority > result.PushingPriority) result = e;
                }

                return result;
            }
        }
    }
}
