using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using System.Collections.Generic;
using Ezilink;
using UnityEngine;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.StoreMenu;

[assembly: MelonInfo(typeof(Ezilink.Ezilink), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Ezilink
{
    public class Ezilink : BloonsTD6Mod
    {
        private readonly List<Tower> ezilis = new();
        private readonly List<Vector2> positions = new();
        private bool upgradingEzilis = false;

        public override void OnApplicationStart()
        {
            ModHelper.Msg<Ezilink>("Ezilink loaded!");
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (upgradingEzilis && ezilis.Count == 0)
            {
                FindEzilis();
                upgradingEzilis = false;
            }
        }

        public override void OnMatchStart()
        {
            base.OnMatchStart();
            FindEzilis();
            positions.Clear();
        }

        public override void OnRestart()
        {
            base.OnRestart();
            ezilis.Clear();
            positions.Clear();
        }

        private void FindEzilis()
        {
            ezilis.Clear();

            foreach (var tower in InGame.instance.GetTowers())
                if (tower.towerModel.baseId == "Ezili")
                    ezilis.Add(tower);
        }

        public override void OnTowerCreated(Tower tower, Entity target, Model modelToUse)
        {
            base.OnTowerCreated(tower, target, modelToUse);

            if (tower.towerModel.baseId != "Ezili") return;

            ezilis.Add(tower);

            if (!Settings.EnableMod) return;

            var model = InGame.instance.GetGameModel().GetTower("Ezili");

            for (int i = positions.Count - 1; i >= 0; i--)
            {
                Vector2 position = positions[i];
                positions.RemoveAt(i);

                if (CanPlaceAtWorld(position, model))
                {                    
                    InGame.instance.bridge.CreateTowerAt(position, model, new Il2CppAssets.Scripts.ObjectId(), false, null, true, false, false);
                    break;
                }
            }

            if (positions.Count == 0)
                RefreshShop();
        }

        public override void OnTowerSold(Tower tower, float amount)
        {
            base.OnTowerSold(tower, amount);

            if (Settings.EnableMod && ezilis.Contains(tower))
            {
                positions.Clear();
                positions.Add(new(tower.Position.X, tower.Position.Y));

                for (int i = ezilis.Count - 1; i >= 0; i--)
                {
                    Tower ezili = ezilis[i];
                    positions.Add(new(ezili.Position.X, ezili.Position.Y));
                    ezilis.Remove(ezili);
                    ezili.SellTower();
                }
            }
        }

        public override void OnTowerDestroyed(Tower tower)
        {
            base.OnTowerDestroyed(tower);

            if (tower.towerModel.baseId == "Ezili")
                ezilis.Remove(tower);
        }

        public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
        {
            base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);

            if (tower.towerModel.baseId != "Ezili" || !Settings.EnableMod || !ezilis.Contains(tower)) return;

            upgradingEzilis = true;
            ezilis.Remove(tower);

            for (int i = ezilis.Count - 1; i >= 0; i--)
            {
                Tower ezili = ezilis[i];
                ezilis.Remove(ezili);

                if (ezili.towerModel.tier < 20)
                    UpgradeTower(ezili.GetTowerToSim(), 0);
            }
        }

        private void UpgradeTower(TowerToSimulation tower, int path)
        {
            var tsm = TowerSelectionMenu.instance;
            var selectedTower = tsm.selectedTower;
            tsm.selectedTower = tower;
            tsm.UpgradeTower(path);
            tsm.selectedTower = selectedTower;
        }

        private static bool CanPlaceAtWorld(Vector2 worldPosition, TowerModel model)
        {
            var inputManager = InGame.instance.InputManager;
            var bridge = InGame.instance.bridge;
            return bridge.CanPlaceTowerAt(worldPosition, model, bridge.MyPlayerNumber, new Il2CppAssets.Scripts.ObjectId());
        }

        private static void RefreshShop()
        {
            ShopMenu.instance.RebuildTowerSet();
            foreach (var button in ShopMenu.instance.ActiveTowerButtons)
            {
                button.Cast<TowerPurchaseButton>().Update();
            }
        }
    }
}
