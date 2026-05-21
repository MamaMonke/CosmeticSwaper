using BepInEx;
using BepInEx.Logging;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CosmeticSwapMod
{
    [BepInPlugin("com.mamamonke.cosmeticswap", "CosmeticSwap", "1.0.0")]
    public class CosmeticSwapMod : BaseUnityPlugin
    {
        private static ManualLogSource _logger;
        private string _itemFilePath;
        private List<string> _cosmeticsToEquip = new List<string>();
        //сохраненные косметики до активации мода
        private CosmeticsController.CosmeticItem[] _savedItems;
        private bool _isEquipped = false;
        private bool _wasButtonPressed = false;
        void Awake()
        {
            _logger = Logger;
            _itemFilePath = Path.Combine(Paths.ConfigPath, "item.txt");
            if (!File.Exists(_itemFilePath))
            {
                File.WriteAllLines(_itemFilePath, new string[]
                {
                    "# Mod By MamaMonke pls don't skid",
                    "# Write cosmetic id you want to put on.",
                    "# Start from a new string every time you have writen in the id.",
                    "# Example bellow (replace with the actual id's you want)",
                    "# LBAGN.",
                    "# LBAGO.",
                    "# LBAGP."
                });
            }
            LoadCosmetics();
        }
        void LoadCosmetics()
        {
            _cosmeticsToEquip.Clear();
            if (!File.Exists(_itemFilePath)) return;
            foreach (string line in File.ReadAllLines(_itemFilePath))
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
                {
                    _cosmeticsToEquip.Add(trimmed);
                }
            }
        }
        void Update()
        {
            if (CosmeticsController.instance == null) return;
            if (GorillaTagger.Instance == null) return;
            if (GorillaTagger.Instance.offlineVRRig == null) return;
            if (!CosmeticsController.instance.allCosmeticsDict_isInitialized) return;
            bool isButtonPressed = ControllerInputPoller.instance.rightControllerSecondaryButton;
            if (isButtonPressed && !_wasButtonPressed)
            {
                OnYButtonPressed();
            }
            _wasButtonPressed = isButtonPressed;
        }
        void OnYButtonPressed()
        {
            if (!_isEquipped)
                EquipCosmetics();
            else
                UnequipCosmetics();
        }
        void EquipCosmetics()
        {
            _savedItems = new CosmeticsController.CosmeticItem[16];
            for (int i = 0; i < 16; i++)
                _savedItems[i] = CosmeticsController.instance.currentWornSet.items[i];
            LoadCosmetics();
            foreach (string cosmeticId in _cosmeticsToEquip)
            {
                CosmeticsController.CosmeticItem item = CosmeticsController.instance.GetItemFromDict(cosmeticId);
                CosmeticsController.instance.PressWardrobeItemButton(item, false, false);
            }
            //Сервер сайд
            CosmeticsController.instance.UpdateWornCosmetics(true);
            _isEquipped = true;
        }
        void UnequipCosmetics()
        {
            if (_savedItems == null)
            {
                _isEquipped = false;
                return;
            }
            //сохранение старой косметики
            for (int i = 0; i < 16; i++)
                CosmeticsController.instance.currentWornSet.items[i] = _savedItems[i];
            //синхронизация с сервером
            CosmeticsController.instance.UpdateWornCosmetics(true);
            _isEquipped = false;
        }
    }
}