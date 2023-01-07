﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WildernessSurvival.Game;

namespace WildernessSurvival.Core
{
    public delegate float ValueFixer(float raw);


    public partial class Player : IAttributeModel, INotifyPropertyChanged
    {
        private const float MaxVisualFuel = 25f;

        public const float MoveStep = 0.02f;

        public Backpack Backpack { get; private set; }
        public IRoute<IPlace> CurRoute;
        private float _healthValue;
        private float _energyValue;
        private float _foodValue;
        private float _waterValue;
        private float _fireFuel;
        private IPlace _location;
        private float _tripProgress;
        private int _actionNumber;
        public readonly AttributeManager Attrs;

        public Player()
        {
            Attrs = new AttributeManager(this);
            Reset();
        }

        public void Reset()
        {
            Health = Food = Water = Energy = AttributeManager.MaxValue;
            _tripProgress = 0;
            CurRoute = Routes.SubtropicsRoute();
            Location = CurRoute.InitialPlace;
            ActionNumber = 0;
            Backpack = new Backpack(this);
        }

        public IList<IItem> AllItems => Backpack.AllItems;

        public IEnumerable<ICookableItem> GetCookableItems() => Backpack.AllItems.OfType<ICookableItem>();
        public IEnumerable<IFuelItem> GetFuelItems() => Backpack.AllItems.OfType<IFuelItem>();

        public IEnumerable<IToolItem> GetToolsOf(ToolType type) =>
            Backpack.AllItems.OfType<IToolItem>().Where(e => e.ToolType == type);

        public bool HasToolOf(ToolType type) =>
            Backpack.AllItems.OfType<IToolItem>().Any(e => e.ToolType == type);

        public IToolItem TryGetBestToolOf(ToolType type) =>
            GetToolsOf(type).OrderByDescending(t => t.Level).FirstOrDefault();


        public IPlace Location
        {
            get => _location;
            set
            {
                if (_location == value) return;
                _location = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocationName)));
            }
        }

        public string LocationName => Location.LocalizedName();

        public int ActionNumber
        {
            get => _actionNumber;
            private set => _actionNumber = value < 0 ? 0 : value;
        }

        public float Health
        {
            get => _healthValue;
            set
            {
                _healthValue = Math.Max(0, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Health)));
            }
        }

        public float Food
        {
            get => _foodValue;
            set
            {
                _foodValue = Math.Max(0, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Food)));
            }
        }

        public float Water
        {
            get => _waterValue;
            set
            {
                _waterValue = Math.Max(0, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Water)));
            }
        }

        public float Energy
        {
            get => _energyValue;
            set
            {
                _energyValue = Math.Max(0, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Energy)));
            }
        }

        public bool HasEnergy => Energy > 0f;

        public float FireFuel
        {
            get => _fireFuel;
            set
            {
                _fireFuel = Math.Max(0, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FireFuelProgress)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FireFuel)));
            }
        }

        public float FireFuelProgress => FireFuel / MaxVisualFuel;

        public float TripProgress
        {
            get => _tripProgress;
            private set
            {
                if (value < 0)
                    _tripProgress = 0;
                else if (value > 1)
                    _tripProgress = 1;
                else
                    _tripProgress = value;
            }
        }

        public bool HasFire => FireFuel > 0f;

        public bool IsDead => Health <= 0;
        public bool IsAlive => !IsDead;
        public bool CanPerformAnyAction => IsAlive && !IsWon;
        public bool IsWon => TripProgress >= 1;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// If the result should be is more than <see cref="MaxValue"/>, the <param name="delta"></param> will be attenuated based on overflow.
        /// </summary>
        public void Modify(AttrType attr, float delta, ValueFixer fixer = null)
        {
            if (fixer != null)
            {
                delta = fixer(delta);
            }

            Attrs.Modify(attr, delta);
        }

        public void SetAttr(AttrType attr, float value) => Attrs.SetAttr(attr, value);

        public float GetAttr(AttrType attr) => Attrs.GetAttr(attr);

        public void AdvanceTrip(float delta = MoveStep) => TripProgress += delta;

        public async Task UseItem(IUsableItem item) => await item.Use(this);

        public void RemoveItem(IItem item) => Backpack.RemoveItem(item);

        public void AddItem(IItem item) => Backpack.AddItem(item);

        public void AddItems(IEnumerable<IItem> items) => Backpack.AddItems(items);
    }
}