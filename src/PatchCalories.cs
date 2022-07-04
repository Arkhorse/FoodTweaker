﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using MelonLoader;

namespace FoodTweaker
{
    class PatchCalories
    {
        [HarmonyPatch(typeof(GearItem), "ApplyBuffs")]
        internal static class ApplyBuffs
        {
            private static void Prefix(GearItem __instance, float normalizedValue)
            {
                if (Settings.settings.modFunction && __instance.m_FoodItem)
                {
                    if (__instance.m_GearName == "GEAR_MRE" && Settings.settings.mreHeating)
                    {
                        if (Mathf.Abs(__instance.m_FoodItem.m_CaloriesRemaining - __instance.m_FoodItem.m_CaloriesTotal * (1 - normalizedValue)) < 1) // Initial self-heating
                        {
                            if (!__instance.m_FreezingBuff)
                            {
                                __instance.m_FreezingBuff = __instance.gameObject.AddComponent<FreezingBuff>();
                            }
                            __instance.m_FreezingBuff.m_InitialPercentDecrease = 10f;
                            __instance.m_FreezingBuff.m_RateOfIncreaseScale = 0.5f;
                            __instance.m_FreezingBuff.m_DurationHours = 2f;
                            __instance.m_FoodItem.m_HeatPercent = 100;
                            __instance.m_FoodItem.m_PercentHeatLossPerMinuteIndoors = 0.5f;
                            __instance.m_FoodItem.m_PercentHeatLossPerMinuteOutdoors = 1f;

                            __instance.m_FreezingBuff.Apply(normalizedValue);
                        }
                        if (__instance.m_FoodItem.IsHot())
                        {
                            __instance.m_FreezingBuff.Apply(normalizedValue);
                        }
                    }
                    if (__instance.m_FoodItem.m_IsMeat || __instance.m_FoodItem.m_IsFish)
                    {
                        if (__instance.m_FoodItem.IsHot())
                        {
                            __instance.m_FreezingBuff.Apply(normalizedValue);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GearItem), "Awake", null)]
        public class AdjustCalories
        {
            private static void Postfix(GearItem __instance)
            {
                if (Settings.settings.modFunction)
                {
                    if (__instance.m_FoodItem)
                    {
                        // Hot Food + Warming Up Buff
                        if (__instance.m_GearName.Contains("Cooked") && (__instance.m_FoodItem.m_IsMeat || __instance.m_FoodItem.m_IsFish))
                        {
                            __instance.m_FoodItem.m_HeatedWhenCooked = true;
                            __instance.m_FoodItem.m_PercentHeatLossPerMinuteIndoors = 1f;
                            __instance.m_FoodItem.m_PercentHeatLossPerMinuteOutdoors = 2f;
                            __instance.m_FoodItem.m_HeatPercent = 100;

                            if (!__instance.m_FreezingBuff)
                            {
                                __instance.m_FreezingBuff = __instance.gameObject.AddComponent<FreezingBuff>();
                            }
                            __instance.m_FreezingBuff.m_InitialPercentDecrease = 10f;
                            __instance.m_FreezingBuff.m_RateOfIncreaseScale = 0.5f;
                            __instance.m_FreezingBuff.m_DurationHours = 1f;
                        }

                        // Meat
                        if (__instance.m_FoodItem.m_IsMeat)
                        {
                            // Shrinkage
                            float bearShrinkage = 1f;
                            float deerShrinkage = 1f;
                            float mooseShrinkage = 1f;
                            float rabbitShrinkage = 1f;
                            float wolfShrinkage = 1f;

                            if (Settings.settings.meatShrinkage == Choice.Realistic)
                            {
                                bearShrinkage = 0.75f;
                                deerShrinkage = 0.75f;
                                mooseShrinkage = 0.75f;
                                rabbitShrinkage = 0.75f;
                                wolfShrinkage = 0.75f;
                            }
                            else if (Settings.settings.meatShrinkage == Choice.Custom)
                            {
                                bearShrinkage = 1 - Settings.settings.bearShrinks;
                                deerShrinkage = 1 - Settings.settings.deerShrinks;
                                mooseShrinkage = 1 - Settings.settings.mooseShrinks;
                                rabbitShrinkage = 1 - Settings.settings.rabbitShrinks;
                                wolfShrinkage = 1 - Settings.settings.wolfShrinks;
                            }

                            // Calories
                            float cookedBearCalories = 900;
                            float cookedDeerCalories = 800;
                            float cookedMooseCalories = 900;
                            float cookedRabbitCalories = 450;
                            float cookedWolfCalories = 700;

                            if (Settings.settings.caloriesMeat == Choice.Realistic)
                            {
                                cookedBearCalories = 1630;
                                cookedDeerCalories = 1450;
                                cookedMooseCalories = 1300;
                                cookedRabbitCalories = 1140;
                                cookedWolfCalories = 1165;
                            }
                            else if (Settings.settings.caloriesMeat == Choice.Custom)
                            {
                                cookedBearCalories = Settings.settings.bearCooked;
                                cookedDeerCalories = Settings.settings.deerCooked;
                                cookedMooseCalories = Settings.settings.mooseCooked;
                                cookedRabbitCalories = Settings.settings.rabbitCooked;
                                cookedWolfCalories = Settings.settings.wolfCooked;
                            }
                            if (Settings.settings.caloriesMeat == Choice.Default && Settings.settings.meatShrinkage != Choice.Default)
                            {
                                cookedBearCalories = 900 / bearShrinkage;
                                cookedDeerCalories = 800 / deerShrinkage;
                                cookedMooseCalories = 900 / mooseShrinkage;
                                cookedRabbitCalories = 450 / rabbitShrinkage;
                                cookedWolfCalories = 700 / wolfShrinkage;
                            }

                            if (Settings.settings.caloriesMeat != Choice.Default || Settings.settings.meatShrinkage != Choice.Default)
                            {
                                if (__instance.m_GearName == "GEAR_RawMeatBear")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedBearCalories * bearShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedMeatBear")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedBearCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawMeatDeer")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedDeerCalories * deerShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedMeatDeer")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedDeerCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawMeatMoose")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedMooseCalories * mooseShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedMeatMoose")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedMooseCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawMeatRabbit")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedRabbitCalories * rabbitShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedMeatRabbit")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedRabbitCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawMeatWolf")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedWolfCalories * wolfShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedMeatWolf")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedWolfCalories;
                                }
                            }
                        }

                        // Fish
                        else if (__instance.m_FoodItem.m_IsFish)
                        {
                            // Shrinkage
                            float salmonShrinkage = 0.66f;
                            float whitefishShrinkage = 0.66f;
                            float troutShrinkage = 0.66f;
                            float bassShrinkage = 0.66f;

                            if (Settings.settings.fishShrinkage == Choice.Realistic)
                            {
                                salmonShrinkage = 0.5f;
                                whitefishShrinkage = 0.5f;
                                troutShrinkage = 0.5f;
                                bassShrinkage = 0.5f;
                            }
                            else if (Settings.settings.fishShrinkage == Choice.Custom)
                            {
                                salmonShrinkage = 1- Settings.settings.salmonShrinks;
                                whitefishShrinkage = 1 - Settings.settings.whitefishShrinks;
                                troutShrinkage = 1 - Settings.settings.troutShrinks;
                                bassShrinkage = 1 - Settings.settings.bassShrinks;
                            }

                            // Calories
                            float cookedSalmonCalories = 454.5f;
                            float cookedWhitefishCalories = 383.5f;
                            float cookedTroutCalories = 383.5f;
                            float cookedBassCalories = 454.5f;

                            if (Settings.settings.caloriesFish == Choice.Realistic)
                            {
                                cookedSalmonCalories = 1780;
                                cookedWhitefishCalories = 1720;
                                cookedTroutCalories = 1690;
                                cookedBassCalories = 1450;
                            }
                            else if (Settings.settings.caloriesFish == Choice.Custom)
                            {
                                cookedSalmonCalories = Settings.settings.salmonCooked;
                                cookedWhitefishCalories = Settings.settings.whitefishCooked;
                                cookedTroutCalories = Settings.settings.troutCooked;
                                cookedBassCalories = Settings.settings.bassCooked;
                            }

                            if (Settings.settings.caloriesFish == Choice.Default && Settings.settings.fishShrinkage != Choice.Default)
                            {
                                cookedSalmonCalories = 454.5f / salmonShrinkage;
                                cookedWhitefishCalories = 383.5f / whitefishShrinkage;
                                cookedTroutCalories = 383.5f / troutShrinkage;
                                cookedBassCalories = 454.5f / bassShrinkage;
                            }

                            if (Settings.settings.caloriesFish != Choice.Default)
                            {
                                if (__instance.m_GearName == "GEAR_RawCohoSalmon")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedSalmonCalories * salmonShrinkage;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedSalmonCalories * salmonShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedCohoSalmon")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedSalmonCalories;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedSalmonCalories;
                                }
                                else if (__instance.m_GearName.Contains("GEAR_RawLakeWhiteFish"))
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedWhitefishCalories * whitefishShrinkage;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedWhitefishCalories * whitefishShrinkage;
                                }
                                else if (__instance.m_GearName.Contains("GEAR_CookedLakeWhiteFish"))
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedWhitefishCalories;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedWhitefishCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawRainbowTrout")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedTroutCalories * troutShrinkage;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedTroutCalories * troutShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedRainbowTrout")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedTroutCalories;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedTroutCalories;
                                }
                                else if (__instance.m_GearName == "GEAR_RawSmallMouthBass")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedBassCalories * bassShrinkage;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedBassCalories * bassShrinkage;
                                }
                                else if (__instance.m_GearName == "GEAR_CookedSmallMouthBass")
                                {
                                    __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cookedBassCalories;
                                    __instance.m_FoodWeight.m_CaloriesPerKG = cookedBassCalories;
                                }
                            }
                        }

                        // Drinks
                        else if (__instance.m_FoodItem.m_IsDrink)
                        {
                            float birchBarkTeaWeight = 0.1f;
                            float coffeeWeight = 0.1f;
                            float goEnergyDrinkWeight = 0.25f;
                            float herbalTeaWeight = 0.1f;
                            float orangeSodaWeight = 0.25f;
                            float reishiTeaWeight = 0.1f;
                            float roseHipTeaWeight = 0.1f;
                            float grapeSodaWeight = 0.25f;
                            float summitSodaWeight = 0.25f;

                            float birchBarkTea = 100 / birchBarkTeaWeight;
                            float coffee = 100 / coffeeWeight;
                            float goEnergyDrink = 100 / goEnergyDrinkWeight;
                            float herbalTea = 100 / herbalTeaWeight;
                            float orangeSoda = 250 / orangeSodaWeight;
                            float reishiTea = 100 / reishiTeaWeight;
                            float roseHipTea = 100 / roseHipTeaWeight;
                            float grapeSoda = 250 / grapeSodaWeight;
                            float summitSoda = 250 / summitSodaWeight;

                            if (Settings.settings.caloriesDrinks == Choice.Realistic)
                            {
                                birchBarkTea = 5 / birchBarkTeaWeight;
                                coffee = 5 / coffeeWeight;
                                goEnergyDrink = 115 / goEnergyDrinkWeight;
                                herbalTea = 2 / herbalTeaWeight;
                                orangeSoda = 160 / orangeSodaWeight;
                                reishiTea = 1 / reishiTeaWeight;
                                roseHipTea = 3 / roseHipTeaWeight;
                                grapeSoda = 170 / grapeSodaWeight;
                                summitSoda = 120 / summitSodaWeight;
                            }
                            else if (Settings.settings.caloriesDrinks == Choice.Custom)
                            {
                                birchBarkTea = Settings.settings.birchBarkTea / birchBarkTeaWeight;
                                coffee = Settings.settings.coffee / coffeeWeight;
                                herbalTea = Settings.settings.herbalTea / herbalTeaWeight;
                                orangeSoda = Settings.settings.orangeSoda / orangeSodaWeight;
                                goEnergyDrink = Settings.settings.goEnergyDrink / goEnergyDrinkWeight;
                                reishiTea = Settings.settings.reishiTea / reishiTeaWeight;
                                roseHipTea = Settings.settings.roseHipTea / roseHipTeaWeight;
                                grapeSoda = Settings.settings.grapeSoda / grapeSodaWeight;
                                summitSoda = Settings.settings.summitSoda / summitSodaWeight;
                            }

                            if (__instance.m_GearName == "GEAR_BirchbarkTea")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * birchBarkTea;
                            }
                            if (__instance.m_GearName == "GEAR_CoffeeCup")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * coffee;
                            }
                            if (__instance.m_GearName == "GEAR_GreenTeaCup")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * herbalTea;
                            }
                            if (__instance.m_GearName == "GEAR_SodaOrange")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * orangeSoda;
                            }
                            if (__instance.m_GearName == "GEAR_SodaEnergy")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * goEnergyDrink;
                            }
                            if (__instance.m_GearName == "GEAR_ReishiTea")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * reishiTea;
                            }
                            if (__instance.m_GearName == "GEAR_RoseHipTea")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * roseHipTea;
                            }
                            if (__instance.m_GearName == "GEAR_SodaGrape")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * grapeSoda;
                            }
                            if (__instance.m_GearName == "GEAR_Soda")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * summitSoda;
                            }
                        }

                        // Other Foods
                        else
                        {
                            float airlineChickenWeight = 0.25f;
                            float airlineVegetableWeight = 0.25f;
                            float beefJerkyWeight = 0.1f;
                            float cattailStalkWeight = 0.05f;
                            float chocolateBarWeight = 0.1f;
                            float condensedMilkWeight = 0.25f;
                            float dogFoodWeight = 0.3f;
                            float energyBarWeight = 0.1f;
                            float granolaBarWeight = 0.1f;
                            float ketchupChipsWeight = 0.1f;
                            float mapleSyrupWeight = 0.3f;
                            float mreWeight = 0.5f;
                            float peanutButterWeight = 0.5f;
                            float pinnaclePeachesWeight = 0.5f;
                            float porkAndBeansWeight = 0.25f;
                            float saltyCrackersWeight = 0.1f;
                            float sardinesWeight = 0.1f;
                            float tomatoSoupWeight = 0.25f;

                            float airlineChicken = 620;
                            float airlineVegetable = 560;
                            float beefJerky = 350;
                            float cattailStalk = 150;
                            float chocolateBar = 250;
                            float condensedMilk = 750;
                            float dogFood = 500;
                            float energyBar = 500;
                            float granolaBar = 300;
                            float ketchupChips = 300;
                            float mapleSyrup = 850;
                            float mre = 1750;
                            float peanutButter = 900;
                            float pinnaclePeaches = 450;
                            float porkAndBeans = 600;
                            float saltyCrackers = 600;
                            float sardines = 300;
                            float tomatoSoup = 300;

                            if (Settings.settings.caloriesOtherFood == Choice.Realistic)
                            {
                                airlineChicken = 620 / airlineChickenWeight;
                                airlineVegetable = 560 / airlineVegetableWeight;
                                beefJerky = 410 / beefJerkyWeight;
                                cattailStalk = 25 / cattailStalkWeight;
                                chocolateBar = 585 / chocolateBarWeight;
                                condensedMilk = 815 / condensedMilkWeight;
                                dogFood = 425 / dogFoodWeight;
                                energyBar = 500 / energyBarWeight;
                                granolaBar = 300 / granolaBarWeight;
                                ketchupChips = 540 /ketchupChipsWeight;
                                mapleSyrup = 920 / mapleSyrupWeight;
                                mre = 1200 / mreWeight;
                                peanutButter = 3060 / peanutButterWeight;
                                pinnaclePeaches = 245 / pinnaclePeachesWeight;
                                porkAndBeans = 265 / porkAndBeansWeight;
                                saltyCrackers = 515 / saltyCrackersWeight;
                                sardines = 230 / sardinesWeight;
                                tomatoSoup = 150 / tomatoSoupWeight;
                            }
                            else if (Settings.settings.caloriesOtherFood == Choice.Custom)
                            {
                                airlineChicken = Settings.settings.airlineChicken;
                                airlineVegetable = Settings.settings.airlineVegetarian;
                                beefJerky = Settings.settings.beefJerky;
                                cattailStalk = Settings.settings.cattailStalk;
                                chocolateBar = Settings.settings.chocolateBar;
                                condensedMilk = Settings.settings.condensedMilk;
                                dogFood = Settings.settings.dogFood;
                                energyBar = Settings.settings.energyBar;
                                granolaBar = Settings.settings.granolaBar;
                                ketchupChips = Settings.settings.ketchupChips;
                                mapleSyrup = Settings.settings.mapleSyrup;
                                mre = Settings.settings.mre;
                                peanutButter = Settings.settings.peanutButter;
                                pinnaclePeaches = Settings.settings.pinnaclePeaches;
                                porkAndBeans = Settings.settings.porkAndBeans;
                                saltyCrackers = Settings.settings.saltyCrackers;
                                sardines = Settings.settings.sardines;
                                tomatoSoup = Settings.settings.tomatoSoup;
                            }

                            if (__instance.m_GearName == "GEAR_AirlineFoodChick")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * airlineChicken;
                            }
                            if (__instance.m_GearName == "GEAR_AirlineFoodVeg")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * airlineVegetable;
                            }
                            if (__instance.m_GearName == "GEAR_BeefJerky") 
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * beefJerky;
                            }
                            if (__instance.m_GearName == "GEAR_CandyBar")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * chocolateBar;
                            }
                            if (__instance.m_GearName == "GEAR_CattailStalk")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * cattailStalk;
                            }
                            if (__instance.m_GearName == "GEAR_CondensedMilk")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * condensedMilk;
                            }
                            if (__instance.m_GearName == "GEAR_DogFood")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * dogFood;
                            }
                            if (__instance.m_GearName == "GEAR_EnergyBar")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * energyBar;
                            }
                            if (__instance.m_GearName == "GEAR_GranolaBar")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * granolaBar;
                            }
                            if (__instance.m_GearName == "GEAR_KetchupChips")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * ketchupChips;
                            }
                            if (__instance.m_GearName == "GEAR_MapleSyrup")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * mapleSyrup;
                            }
                            if (__instance.m_GearName == "GEAR_MRE")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * mre;
                            }
                            if (__instance.m_GearName == "GEAR_PeanutButter")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * peanutButter;
                            }
                            if (__instance.m_GearName == "GEAR_PinnacleCanPeaches")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * pinnaclePeaches;
                            }
                            if (__instance.m_GearName == "GEAR_CannedBeans")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * porkAndBeans;
                            }
                            if (__instance.m_GearName == "GEAR_Crackers")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * saltyCrackers;
                            }
                            if (__instance.m_GearName == "GEAR_CannedSardines")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * sardines;
                            }
                            if (__instance.m_GearName == "Gear_TomatoSoupCan")
                            {
                                __instance.m_FoodItem.m_CaloriesRemaining = __instance.m_WeightKG * tomatoSoup;
                            }
                        }
                        __instance.m_FoodItem.m_CaloriesTotal = __instance.m_FoodItem.m_CaloriesRemaining;
                    }
                }
            }
        }
    }
}
