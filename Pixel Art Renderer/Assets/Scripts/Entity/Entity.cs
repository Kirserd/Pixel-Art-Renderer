using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MIRAI.Scripts.Entity
{
    public class Entity : MonoBehaviour
    {
        public int Level { get; protected set; }

        // UNIFORM ATTRIBUTES
        public Attribute Strength { get; protected set; }
        public Attribute Agility { get; protected set; }
        public Attribute Vitality { get; protected set; }
        public Attribute Intelligence { get; protected set; }
        public Attribute Faith { get; protected set; }
        public Attribute Dexterity { get; protected set; }
        public Attribute Luck { get; protected set; }

        //HIDDEN ATTRIBUTES
        public Attribute Speed { get; protected set; }
        public Attribute Learning { get; protected set; }
        public Attribute Tendency { get; protected set; }
        //...

        //FEATS
        public List<Feat> Feats = new();

        //READONLY STATS
        public int MaxHP => (int)(Vitality.Final * 48f);
        public int MaxMP => (int)((Intelligence.Base > Faith.Base? 
                                   Intelligence.Final * 0.8f + Faith.Final * 0.2f : 
                                   Intelligence.Final * 0.2f + Faith.Final * 0.8f) * 48f);
        //...

        //RUNTIME STATS
        [Header("Runtime stats")]
        [SerializeField] private float _hp;        
        [SerializeField] private float _mp;
        //...

        public float HP
        {
            get => _hp;
            set
            {
                //...
                _hp = value;
            }
        }
        public float MP
        {
            get => _mp;
            set
            {
                //...
                _mp = value;
            }
        }
        //...
    }
}