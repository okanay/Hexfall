using System;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    
    [HideInInspector]
    public List<Hexagon> listHexagons;
    [HideInInspector]
    public List<HexPot> listHexPots;
    [HideInInspector]
    public List<HexPot> listSelectedPots = new List<HexPot>();
    [HideInInspector]
    public List<Hexagon> listMatchHexagons;
    [HideInInspector] 
    public List<int> listSelectedPotIDs = new List<int>();
    [HideInInspector]
    public List<Hexagon> listPreviousSelectedHexagons = new List<Hexagon>();
    [HideInInspector] 
    public List<Hexagon> listBombHexagons = new List<Hexagon>();
    [HideInInspector]
    public List<HexPot> listPotentials = new List<HexPot>();
    [HideInInspector]
    public int colorVariableCount;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;

    [HideInInspector]
    public float yOffset = 2.25f;
    [HideInInspector]
    public float xOffSet = 2;
    [HideInInspector]
    public float yExtendedOffSet = 1.12f;
    
    public Color[] hexagonColors;
    
    public Transform hexagonParticle, centerDot;
    public Transform hexagonPrefab, hexPotPrefab;
    public Transform hexagonParent, potParent;
}
