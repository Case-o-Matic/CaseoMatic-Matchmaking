using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    [Serializable]
    public class MatchmakingSearchSettings
    {
        public readonly List<MatchmakingSearchSettingsCondition> conditions;

        public MatchmakingSearchSettings(List<MatchmakingSearchSettingsCondition> _conditions)
        {
            conditions = _conditions;
        }
    }

    [Serializable]
    public struct MatchmakingSearchSettingsCondition
    {
        public readonly object conditionObject;

        public MatchmakingSearchSettingsCondition(object _condition)
        {
           if(_condition.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length == 0)
               throw new InvalidOperationException("Not serializable types are invalid for matchmaking conditions.");

            conditionObject = _condition;
        }

        public override bool Equals(object obj)
        {
            return conditionObject.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
