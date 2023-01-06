﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WildernessSurvival.Game;

namespace WildernessSurvival.Core
{
    public enum AttrType
    {
        Health,
        Food,
        Water,
        Energy
    }

    public delegate float ValueFixer(float raw);

    public partial class Player : INotifyPropertyChanged
    {
        private const float MaxValue = 1f;

        public const float MoveStep = 0.05f;

        private Backpack _backpack;

        public IRoute<IPlace> CurRoute;
        private float _healthValue;
        private float _energyValue;
        private float _foodValue;
        private float _waterValue;
        private bool _hasFire;
        private IPlace _location;
        private float _tripRatio;
        private int _turnNumber;

        public Player()
        {
            Reset();
        }

        public void Reset()
        {
            Health = Food = Water = Energy = MaxValue;
            HasFire = false;
            _tripRatio = 0;
            CurRoute = Routes.SubtropicsRoute();
            Location = CurRoute.InitialPlace;
            TurnCount = 0;
            _backpack = new Backpack(this);
        }

        public IList<IItem> AllItems => _backpack.AllItems;

        public IList<IRawItem> GetRawItems() => _backpack.GetRawItems();

        public IList<IHuntingToolItem> GetHuntingTools() => _backpack.GetHuntingTools();

        public IHuntingToolItem GetBestHuntingTool()
        {
            return GetHuntingTools().OrderByDescending(t => t.Level).FirstOrDefault();
        }

        public bool HasWood => _backpack.HasWood;

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

        public int TurnCount
        {
            get => _turnNumber;
            private set => _turnNumber = value < 0 ? 0 : value;
        }

        public float Health
        {
            get => _healthValue;
            private set
            {
                if (value > MaxValue)
                    _healthValue = MaxValue;
                else if (value < 0)
                    _healthValue = 0;
                else
                    _healthValue = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Health)));
            }
        }

        public float Food
        {
            get => _foodValue;
            private set
            {
                if (value > MaxValue)
                    _foodValue = MaxValue;
                else if (value < 0)
                    _foodValue = 0;
                else
                    _foodValue = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Food)));
            }
        }

        public float Water
        {
            get => _waterValue;
            private set
            {
                if (value > MaxValue)
                    _waterValue = MaxValue;
                else if (value < 0)
                    _waterValue = 0;
                else
                    _waterValue = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Water)));
            }
        }

        public float Energy
        {
            get => _energyValue;
            private set
            {
                if (value > MaxValue)
                    _energyValue = MaxValue;
                else if (value < 0)
                    _energyValue = 0;
                else
                    _energyValue = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Energy)));
            }
        }

        public float TripRatio
        {
            get => _tripRatio;
            private set
            {
                if (value < 0)
                    _tripRatio = 0;
                else if (value > 1)
                    _tripRatio = 1;
                else
                    _tripRatio = value;
            }
        }

        public bool HasFire
        {
            get => _hasFire;
            set
            {
                if (_hasFire == value) return;
                _hasFire = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasFire)));
            }
        }

        public bool HasFishingTool => _backpack.HasFishingTool;

        public bool HasOxe => _backpack.HasOxe;

        public bool HasHuntingTool => _backpack.HasHuntingTool;

        public bool IsDead => Health <= 0 || Food <= 0 || Water <= 0 || Energy <= 0;
        public bool IsAlive => !IsDead;
        public bool CanPerformAnyAction => IsAlive && !IsWon;
        public bool IsWon => TripRatio >= 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Modify(float delta, AttrType type, ValueFixer fixer = null)
        {
            if (fixer != null)
            {
                delta = fixer(delta);
            }

            switch (type)
            {
                case AttrType.Health:
                    Health += delta;
                    break;
                case AttrType.Food:
                    Food += delta;
                    break;
                case AttrType.Water:
                    Water += delta;
                    break;
                case AttrType.Energy:
                    Energy += delta;
                    break;
            }
        }

        public void AdvanceTrip(float delta = MoveStep) => TripRatio += delta;

        public void UseItem(IUsableItem item) => item.Use(this);

        public void RemoveItem(IItem item) => _backpack.Remove(item);

        public void AddItem(IItem item) => _backpack.AddItem(item);

        public void AddItems(IEnumerable<IItem> items) => _backpack.AddItems(items);

        public void ConsumeWood(int Count) => _backpack.ConsumeWood(Count);
    }
}