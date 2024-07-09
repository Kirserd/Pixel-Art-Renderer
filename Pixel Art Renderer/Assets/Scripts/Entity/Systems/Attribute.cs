using System;

namespace MIRAI.Scripts.Entity
{
    public class Attribute
    {
        //==========================================================//
        // TODO:                                                    //
        //   finish ModifierTypes, ModifierMask, Modifier logic,    //
        //   encapsulate all of it.                                 //
        //   Then finish computation of Final.                      //                  
        //==========================================================//

        /// <summary>
        /// Final - applies to the base affected by other "Final" masked modifiers, and adds to the final. |
        /// DirtyBase - applies to the clean base value and adds to the final before calculating "DirtyFinal". |
        /// DirtyFinal - applies to the base affected by "DirtyBase" masked modifiers, and adds to the final. |
        /// </summary>
        public enum ModifierMask
        {
            Final,
            DirtyBase,
            DirtyFinal
        }
        public enum ModifierType
        {
            Empty,
            Add,
            Multiply,
            Power,
            //...
        }
        public enum ModifierGroup
        {
            Positive,
            Neutral,
            Negative
        }
        public struct Modifier
        {
            public const ushort EmptyId = ushort.MaxValue;

            public readonly ushort Id;
            public readonly ModifierGroup Group;
            public readonly ModifierMask Mask;
            public readonly ModifierType Type;
            public readonly float Value;

            public Modifier(ushort id, float value, ModifierType type, ModifierMask mask, ModifierGroup group)
            {
                Id = id;
                Value = value;
                Type = type;
                Mask = mask;
                Group = group;
            }
            public Modifier(byte _)
            {
                Id = EmptyId;
                Value = -1;
                Type = ModifierType.Empty;
                Mask = ModifierMask.Final;
                Group = ModifierGroup.Neutral;
            }

            public override bool Equals(object obj)
                => obj is Modifier modifier &&
                   Id == modifier.Id;
            public override int GetHashCode() 
                => HashCode.Combine(Type, Value);
            public override string ToString()
                => $"[ id => {Id} : type => {Type} : value => {Value} ]";

            public static bool operator ==(Modifier a, Modifier b) => a.Equals(b);
            public static bool operator !=(Modifier a, Modifier b) => !a.Equals(b);
        }

        public float Base;
        public float Final 
        { 
            get
            {
                if (_isDirty)
                    RecalculateFinal();

                return _final;
            }
            private set => _final = value;
        }

        private float _final;
        private bool _isDirty;

        private const byte MODIFIERS_MIN_CAPACITY = 4;
        private const byte MODIFIERS_MAX_CAPACITY = 32;
        private Modifier[] _modifiers;

        public Attribute(float @base)
        {
            Base = @base;
            Final = @base;

            _modifiers = new Modifier[MODIFIERS_MIN_CAPACITY];
            InitializeModifiersArray();
        }
        private void InitializeModifiersArray()
        {
            for (int i = 0; i < _modifiers.Length; i++)
                _modifiers[i] = new Modifier(0);
        }
        private byte ScaleModifiersArray(byte lackingCapacity = 1)
        {
            if (_modifiers.Length == MODIFIERS_MAX_CAPACITY)
                return 0;
            else if (_modifiers.Length + lackingCapacity > MODIFIERS_MAX_CAPACITY)
                lackingCapacity = (byte)(MODIFIERS_MAX_CAPACITY - _modifiers.Length);

            var buffer = new Modifier[_modifiers.Length + lackingCapacity];
            _modifiers.CopyTo(buffer, 0);
            for (int i = _modifiers.Length; i < buffer.Length; i++)
                buffer[i] = new Modifier(0);

            _modifiers = buffer;
            return lackingCapacity;
        }
        public void AddModifier(Modifier modifier)
        {
            for (byte i = 0; i < _modifiers.Length; i++)
            {
                if (_modifiers[i].Id == Modifier.EmptyId) {
                    _modifiers[i] = modifier;
                    _isDirty = true;
                    return;
                }
            }
            byte prevCapacity = (byte)_modifiers.Length;
            
            if (ScaleModifiersArray() == 0)
                return;

            _modifiers[prevCapacity + 1] = modifier;
            _isDirty = true;
        }
        public void AddModifiers(Modifier[] modifiers)
        {
            byte lastNotEmpty = byte.MinValue;
            for (byte i = 0; i < _modifiers.Length; i++)
            {       
                if (_modifiers[i].Id == Modifier.EmptyId)
                {
                    lastNotEmpty = i;
                    break;
                }
            }
            byte lackingCapacity = (byte)(
                lastNotEmpty == 0 ? 
                modifiers.Length : 
                modifiers.Length - (_modifiers.Length - (lastNotEmpty + 1)));

            if(lackingCapacity < 0)
                for (int i = 0; i < modifiers.Length; i++)
                    _modifiers[lastNotEmpty + i + 1] = modifiers[i];

            byte prevCapacity = (byte)_modifiers.Length;
            byte addedCapacity = ScaleModifiersArray(lackingCapacity);
            if (addedCapacity == 0)
                return;

            for (int i = 0; i < addedCapacity + 1; i++)
                _modifiers[prevCapacity + i + 1] = modifiers[i];
            _isDirty = true;
        }
        public void RemoveModifier(ushort modifierId)
        {
            for (byte i = 0; i < _modifiers.Length; i++)
            {
                if (_modifiers[i].Id == modifierId)
                    _modifiers[i] = new Modifier(0);
            }
            _isDirty = true;
        }
        public void RemoveModifiers(ushort[] modifierIds)
        {
            for (byte i = 0; i < _modifiers.Length; i++)
                for (byte j = 0; j < modifierIds.Length; j++)
                    if (_modifiers[i].Id == modifierIds[j])
                        _modifiers[i] = new Modifier(0);

            _isDirty = true;
        }
        private void RecalculateFinal()
        {
            //TODO: Actual modifier applying logic
            _isDirty = false;
            _final = Base;
        }
    }
}