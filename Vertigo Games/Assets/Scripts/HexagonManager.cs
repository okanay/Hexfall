using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexagonManager : Data
{
    // ---------------------------------------------------------Private Setter Properties
    
    [SerializeField] 
    private Transform hexagonSelectedBarrier;
    
    private readonly List<Hexagon> m_ListHexagonPool = new List<Hexagon>();
    private readonly List<HexPot> m_ListHexPotPool = new List<HexPot>();
    private readonly List<int> m_CheckNumber = new List<int> {0, 1};
    private readonly List<Hexagon> m_MatchHexagon = new List<Hexagon>();
    
    private int m_BombRequiredScore = 1000;
    private bool m_YExtended;
    private bool m_AnyMatchesCheck;
    private bool m_Clockwise;
    private Hexagon m_MainHexagon;
    private Hexagon m_SecondHexagon;
    private Hexagon m_ThirdHexagon;
    private int m_NumberOfExplodingCount;
    private int m_BarrierTypeValue;
   
    public bool ExplodeProtect { get; private set; }
    public bool SwipeProtect { get; private set; }
    public int Moves { get; private set; }
    public bool GameOver { get; private set; }

    public static HexagonManager Instance;

    // ---------------------------------------------------------Functions
    
    private void Awake()
    {
        colorVariableCount = 5;
        Instance = this;
    }
    private void HexagonParticleCreate(Transform targetPosition, int colorID)
    {
        int randomCount = Random.Range(3, 5);
        for (int i = 0; i < randomCount; i++)
        {
            var newParticle = Instantiate(hexagonParticle, targetPosition.position, targetPosition.rotation);
            var hexParticle = newParticle.GetComponent<HexParticle>();
            hexParticle.ParticleInitialize(colorID);
        }
    }
    private void CreateBoardPots()
    {
        for (var i = 0; i < width; i++)
        {
            if (i % 2 == 0)
                m_YExtended = false;
            else
                m_YExtended = true;

            for (var j = 0; j < height; j++) BoardPotsCreate(i, j, m_YExtended, FirstHexPotPosition());
        }
    }
    private void BoardPotsCreate(int xOffSetMultiply, int yOffSetMultiply, bool extendedOffSetCheck, Vector2 additionalHexagonPos)
    {
        var targetPosX = xOffSetMultiply * xOffSet + additionalHexagonPos.x;
        var targetPosY = yOffSetMultiply * yOffset + additionalHexagonPos.y;

        var targetPos = new Vector2(targetPosX, targetPosY);

        var newPot = Instantiate(hexPotPrefab, transform.position, Quaternion.identity, potParent);
        var hexPot = newPot.GetComponent<HexPot>();

        listHexPots.Add(hexPot);

        hexPot.SetInitializePos(targetPos.x, targetPos.y, extendedOffSetCheck);
        hexPot.potID = listHexPots.Count - 1;
    }
    private void CreateInitializeHexagons()
    {
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
            HexagonInstantiate();
    }
    private void HexagonInstantiate()
    {
        var newHex = Instantiate(hexagonPrefab, Vector3.up * 45, Quaternion.identity, hexagonParent);
        var hexagon = newHex.GetComponent<Hexagon>();

        listHexagons.Add(hexagon);
        hexagon.SetMyColor(Random.Range(0, colorVariableCount));
    }
    private void HexagonsInitializePosition(bool startInitialize)
    {
        for (var i = 0; i < listHexPots.Count; i++) listHexagons[i].SetPotData(listHexPots[i], startInitialize);
    }
    private void BoardInitializeMatchProtect()
    {
        for (var i = 0; i < listHexagons.Count; i++)
        {
            listHexagons[i].SetMyColor(Random.Range(0, colorVariableCount));
        }
        
        for (var i = 0; i < listHexagons.Count; i++)
        {
            var iteration = 0;
            ColorMatchCheck(listHexPots[i], false);
            while (m_AnyMatchesCheck && iteration < 500)
            {
                iteration++;
                listHexagons[i].SetMyColor(Random.Range(0, colorVariableCount));
                ColorMatchCheck(listHexPots[i], false);
            }
        }
        
        MatchCheck("PotentialCheck",true,listHexPots[0],listHexPots);
    }
    private void HexagonsExplode()
    {
        if (listMatchHexagons.Count <= 0) return;
        if (ExplodeProtect) return;

        ExplodeProtect = true;
        BoostDeActivate();
        m_ListHexagonPool.Clear();
        m_ListHexPotPool.Clear();
        listSelectedPotIDs.Clear();
        listPreviousSelectedHexagons.Clear();

        foreach (var hexagon in listMatchHexagons)
        {
            var transform = hexagon.transform;
            var colorID = hexagon.GetMyColorID();

            HexagonParticleCreate(transform, colorID);
        }

        foreach (var hexagon in listMatchHexagons)
        {
            int targetHexPot = hexagon.GetMyPotID();
            listHexPots[targetHexPot].nullCheck = true;
            hexagon.transform.position = Vector3.up * 45;
            if (!m_ListHexagonPool.Contains(listHexPots[targetHexPot].myHexagon))
            {
                m_ListHexagonPool.Add(listHexPots[targetHexPot].myHexagon);

            }
        }

        CanvasManager.Instance.CurrentScoreChange(listMatchHexagons.Count * 5);

        foreach (var matchHexagon in listMatchHexagons)
        {
            if (matchHexagon.BombActive)
            {
                matchHexagon.HexagonTypeChange("Hexagon");
                matchHexagon.CountdownSet(+1);
                m_NumberOfExplodingCount++;
            }
        }

        var additionalBombListCount = m_BombListCount();
        for (int i = 0; i < additionalBombListCount; i++)
        {
            try
            {
                if (!listBombHexagons.Contains(listMatchHexagons[i]))
                {
                    listMatchHexagons[i].HexagonTypeChange("Bomb");
                    listMatchHexagons[i].CountdownSet(Random.Range(5, 15));
                }
            }
            catch (Exception e)
            {
                Debug.Log("The number of matches is not enough to add bombs.");
            }
        }

        StartCoroutine(nameof(DecreaseHexagonYPos));
    }
    private void ColorMatchCheck(HexPot targetPot, bool checkall)
    {
        listMatchHexagons.Clear();
        MatchCheck("ColorCheck", checkall,targetPot,listHexPots);
        
        if (listMatchHexagons.Count > 0)
            m_AnyMatchesCheck = true;
        else 
            m_AnyMatchesCheck = false;
    }
    private int m_BombListCount()
    {
        var score = CanvasManager.Instance.currentScore;
        if (score < m_BombRequiredScore) return 0;
        return Mathf.Clamp(score / m_BombRequiredScore - listBombHexagons.Count - m_NumberOfExplodingCount,0,100);
    }
    private void MatchPotentialCheck()
    {
        var mainColorID = m_MainHexagon.GetMyColorID();
        var secondColorID = m_SecondHexagon.GetMyColorID();
        var thirdColorID = m_ThirdHexagon.GetMyColorID();

        m_MainHexagon.SetMyColor(secondColorID);
        m_SecondHexagon.SetMyColor(thirdColorID);
        m_ThirdHexagon.SetMyColor(mainColorID);
        
        
        ColorMatchCheck(listHexPots[m_MainHexagon.GetMyPotID()], false);
        ColorMatchCheck(listHexPots[m_SecondHexagon.GetMyPotID()], false);
        ColorMatchCheck(listHexPots[m_ThirdHexagon.GetMyPotID()], false);

        m_MainHexagon.SetMyColor(mainColorID);
        m_SecondHexagon.SetMyColor(secondColorID);
        m_ThirdHexagon.SetMyColor(thirdColorID);

        if (listMatchHexagons.Count > 0)
        {
            foreach (var potentials in listMatchHexagons) { listPotentials.Add(listHexPots[potentials.GetMyPotID()]); }
            ColorMatchCheck(listHexPots[m_MainHexagon.GetMyPotID()], false);
            return;
        }

        m_MainHexagon.SetMyColor(thirdColorID);
        m_SecondHexagon.SetMyColor(mainColorID);
        m_ThirdHexagon.SetMyColor(secondColorID);

        ColorMatchCheck(listHexPots[m_MainHexagon.GetMyPotID()], false);
        ColorMatchCheck(listHexPots[m_SecondHexagon.GetMyPotID()], false);
        ColorMatchCheck(listHexPots[m_ThirdHexagon.GetMyPotID()], false);

        m_MainHexagon.SetMyColor(mainColorID);
        m_SecondHexagon.SetMyColor(secondColorID);
        m_ThirdHexagon.SetMyColor(thirdColorID);

        if (listMatchHexagons.Count > 0)
        {
            foreach (var potentials in listMatchHexagons) { listPotentials.Add(listHexPots[potentials.GetMyPotID()]); }
            ColorMatchCheck(listHexPots[m_MainHexagon.GetMyPotID()], false);
            return;
        }
        
        ColorMatchCheck(listHexPots[m_MainHexagon.GetMyPotID()], false); // reset color id. 
    }
    private Vector2 FirstHexPotPosition()
    {
        var xMultiply = width - 1 / 2;
        var clampDifferent = 9 - width;
        var clampHeight = height;
        if (height % 2 != 0) { clampHeight += 1; } 
        clampHeight = clampHeight / 2;
        return new Vector2(Mathf.Clamp(xMultiply * -2, -8f + clampDifferent, 8), -10f + -clampHeight);
    }
    private IEnumerator LevelStartHexagonsInitializeCo(bool startInitialize)
    {
        HexagonsInitializePosition(startInitialize);

        yield return new WaitForEndOfFrame();

        BoardInitializeMatchProtect();
        
        var whileProtect = 0;
        while (listPotentials.Count <= 0 & whileProtect < 100)
        {
            whileProtect++;
            BoardInitializeMatchProtect();
        }
        
        for (var i = 0; i < listHexagons.Count; i++)
        {
            if (Mathf.Abs(height - i) % height == 0) yield return new WaitForSeconds(0.025f);

            yield return new WaitForSeconds(0.001f);
            listHexagons[i].LerpStateCheck();
        }
    }
    private IEnumerator HexagonSwipe()
    {
       
        var target1 = listSelectedPots[0].myHexagon;
        var target2 = listSelectedPots[1].myHexagon;
        var target3 = listSelectedPots[2].myHexagon;

        var targetHexPot1 = listHexPots[target1.GetMyPotID()];
        var targetHexPot2 = listHexPots[target2.GetMyPotID()];
        var targetHexPot3 = listHexPots[target3.GetMyPotID()];

        if (m_Clockwise)
        {
            target1.SetPotData(targetHexPot2, false);
            target2.SetPotData(targetHexPot3, false);
            target3.SetPotData(targetHexPot1, false);
        }
        else if (!m_Clockwise)
        {
            target1.SetPotData(targetHexPot3, false);
            target2.SetPotData(targetHexPot1, false);
            target3.SetPotData(targetHexPot2, false);
        }
        
        ColorMatchCheck(listHexPots[0], true);
        
        yield return new WaitForSeconds(0.2f);
        
       if (listMatchHexagons.Count > 0)
        {
            Moves++;
            foreach (var bombHexagon in listBombHexagons)
            {
                bombHexagon.CountdownSet(-1);
            }
            
            HexagonsExplode();
            SwipeProtect = false;
        }
       
       SwipeProtect = false;
    }
    private IEnumerator DecreaseHexagonYPos()
    {
        if (GameOver) yield break;
        yield return new WaitForSeconds(0.25f);
        
        var nullCounter = 0;

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var target = j + i * height;

                if (listHexPots[target].nullCheck)
                {
                    nullCounter++;
                }
                else if (nullCounter > 0)
                {
                    var newTarget = target - nullCounter;
                    listHexPots[target].myHexagon.SetPotData(listHexPots[newTarget], false);
                    listHexPots[newTarget].myHexagon.LerpStateCheck();
                }

                for (var k = 0; k < nullCounter; k++)
                {
                    var targetPot = height + i * height - nullCounter;
                    if (!m_ListHexPotPool.Contains(listHexPots[targetPot]))
                        m_ListHexPotPool.Add(listHexPots[targetPot]);
                }
            }

            nullCounter = 0;
        }

        yield return new WaitForSeconds(0.2f);

        for (var i = m_ListHexagonPool.Count - 1; i >= 0; i--)
        {
            m_ListHexagonPool[i].SetPotData(m_ListHexPotPool[i], true);

            var iteration = 0;
            m_ListHexagonPool[i].SetMyColor(Random.Range(0, colorVariableCount));

            m_ListHexagonPool[i].LerpStateCheck();
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var pots in listHexPots) pots.nullCheck = false;
        ColorMatchCheck(listHexPots[0], true);

        if (listMatchHexagons.Count > 1)
        {
            yield return new WaitForSeconds(0.35f);
            ExplodeProtect = false;
            HexagonsExplode();
        }
        else
        {
            yield return new WaitForSeconds(0.35f);
            MatchCheck("PotentialCheck", true,listHexPots[0],listHexPots);
            if (listPotentials.Count <= 0)
            {
                GameOverCall();
            }
            else
            {
                ExplodeProtect = false;
                listPotentials.Clear();
            }
        }
    }
    private IEnumerator GameOverCo()
    {
        var listCount = listHexagons.Count;
        
        for (var i = 0; i < listCount; i++)
        {
            var randomHex = listHexagons.Random();
            var randomHexID = randomHex.GetMyPotID();
            var randomPot = listHexPots[randomHexID];

            listHexagons.Remove(randomHex);

            randomHex.SetPotData(randomPot, true);
            HexagonParticleCreate(randomPot.transform, randomHex.GetMyColorID());
            
            yield return new WaitForSeconds(0.005f);
        }

        yield return new WaitForSeconds(0.5f);
        
        CanvasManager.Instance.GameOverMenu();

    }
    public void OnGameStart()
    {
        CreateBoardPots();
        CreateInitializeHexagons();
        StartCoroutine(LevelStartHexagonsInitializeCo(true));
        ColorMatchCheck(listHexPots[0], true);
    }
    public void GameOverCall()
    {
        if (GameOver) return;
        GameOver = true;
        StartCoroutine(nameof(GameOverCo));
    }
    public void BombRequiredSet(int setter)
    {
        m_BombRequiredScore = setter;
    }
    public void ClockWise(bool clockwiseTrue)
    {
        if (SwipeProtect) return;
        SwipeProtect = true;

        m_Clockwise = clockwiseTrue;
        StartCoroutine(nameof(HexagonSwipe));
    }
    public void BoostActivate()
    {
        foreach (var selected in listSelectedPots)
        {
            selected.myHexagon.transform.localScale = Vector3.one * 1.05f;
            listHexPots[selected.potID].SelectedColor(true); 
        } 
        
        var totalX = 0f;
        var totalY = 0f;
            
        foreach(var selected in listSelectedPots) 
        { 
            totalX += selected.GetMyVector().x;
            totalY += selected.GetMyVector().y;
        }
            
        var centerX = totalX / listSelectedPots.Count;
        var centerY = totalY / listSelectedPots.Count;

        centerDot.position = new Vector3(centerX, centerY, -0.15f);

        if (m_BarrierTypeValue == 1)
        {
            hexagonSelectedBarrier.transform.localPosition = new Vector3(0.88f, 0.04f, 0);
            hexagonSelectedBarrier.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (m_BarrierTypeValue == 2)
        {
            hexagonSelectedBarrier.transform.localPosition = Vector3.zero + Vector3.left * 0.5f;
            hexagonSelectedBarrier.transform.eulerAngles = new Vector3(0, 0, -180);
        }
        else if (m_BarrierTypeValue == 3)
        {
            hexagonSelectedBarrier.transform.localPosition = new Vector3(0.88f, 0.04f, 0);
            hexagonSelectedBarrier.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (m_BarrierTypeValue == 4)
        {
            hexagonSelectedBarrier.transform.localPosition = Vector3.zero + Vector3.left * 0.6f;
            hexagonSelectedBarrier.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        else if (m_BarrierTypeValue == 5)
        {
            hexagonSelectedBarrier.transform.localPosition = Vector3.zero + Vector3.right * 0.6f;
            hexagonSelectedBarrier.transform.eulerAngles = new Vector3(0.88f, 0.04f, 0);
        }
    }
    public void BoostDeActivate()
    {
        foreach (var selected in listSelectedPots)
        {
            selected.myHexagon.transform.localScale = Vector3.one * 0.98f;
            listHexPots[selected.potID].SelectedColor(false);
        }

        listSelectedPots.Clear();
        centerDot.position = Vector3.left * -60;
    }
    public void MatchCheck<T>(string checkType, bool checkAll, HexPot targetHexPot, List<T> targetList) // add like this listMatchHexagons.Clear();
    {
        var checkCount = 1;
        if (checkAll) checkCount = targetList.Count;

        for (int i = 0; i < checkCount; i++)
        {
            Hexagon hexagon;
            
            if (!checkAll)
            {
                hexagon = targetHexPot.myHexagon;
            }
            else
            {
                targetHexPot = listHexPots[i];
                hexagon = listHexPots[i].myHexagon;
            }

            m_MatchHexagon.Clear();

            if (listPotentials.Count > 0 & checkType == "PotentialCheck")
            {
                return;
            }

            for (int j = 0; j < 6; j++)
            {
                m_MatchHexagon.Clear();

                for (int k = 0; k < 2; k++)
                {
                    if (m_CheckNumber[k] == 0 & m_MatchHexagon.Count < 2)
                    {
                        var upsideX = targetHexPot.GetMyVector().x + 0;
                        var upsideY = targetHexPot.GetMyVector().y + yOffset;
                        var upsidePos = new Vector2(upsideX, upsideY);
                        var upsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == upsidePos);
                        if (upsidePot != null)
                        {
                            var upsideHex = upsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (upsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(upsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_BarrierTypeValue = 0;
                                m_MatchHexagon.Add(upsideHex);   
                            }
                        }
                    }

                    if (m_CheckNumber[k] == 1 & m_MatchHexagon.Count < 2)
                    {
                        var rightUpsideX = targetHexPot.GetMyVector().x + xOffSet;
                        var rightUpsideY = targetHexPot.GetMyVector().y + yExtendedOffSet;
                        var rightUpsidePos = new Vector2(rightUpsideX, rightUpsideY);
                        var rightUpsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == rightUpsidePos);
                        if (rightUpsidePot != null)
                        {
                            var rightUpsideHex = rightUpsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (rightUpsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(rightUpsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_BarrierTypeValue = 1;
                                m_MatchHexagon.Add(rightUpsideHex);   
                            }
                        }
                    }

                    if (m_CheckNumber[k] == 2 & m_MatchHexagon.Count < 2)
                    {
                        var rightDownsideX = targetHexPot.GetMyVector().x + xOffSet;
                        var rightDownsideY = targetHexPot.GetMyVector().y - yExtendedOffSet;
                        var rightDownsidePos = new Vector2(rightDownsideX, rightDownsideY);
                        var rightDownsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == rightDownsidePos);
                        if (rightDownsidePot != null)
                        {
                            var rightDownsideHex = rightDownsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (rightDownsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(rightDownsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_MatchHexagon.Add(rightDownsideHex);
                                m_BarrierTypeValue = 2;
                            }
                        }
                    }

                    if (m_CheckNumber[k] == 3 & m_MatchHexagon.Count < 2)
                    {
                        var downsideX = targetHexPot.GetMyVector().x + 0;
                        var downsideY = targetHexPot.GetMyVector().y - yOffset;
                        var downsidePos = new Vector2(downsideX, downsideY);
                        var downsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == downsidePos);
                        if (downsidePot != null)
                        {
                            var downsideHex = downsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (downsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(downsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_MatchHexagon.Add(downsideHex);  
                                m_BarrierTypeValue = 3;
                            }
                        }
                    }

                    if (m_CheckNumber[k] == 4 & m_MatchHexagon.Count < 2)
                    {
                        var leftDownsideX = targetHexPot.GetMyVector().x - xOffSet;
                        var leftDownsideY = targetHexPot.GetMyVector().y - yExtendedOffSet;
                        var leftDownsidePos = new Vector2(leftDownsideX, leftDownsideY);
                        var leftDownsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == leftDownsidePos);
                        if (leftDownsidePot != null)
                        {
                            var leftDownsideHex = leftDownsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (leftDownsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(leftDownsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_MatchHexagon.Add(leftDownsideHex);  
                                m_BarrierTypeValue = 4;
                            }
                        }
                    }

                    if (m_CheckNumber[k] == 5 & m_MatchHexagon.Count < 2)
                    {
                        var leftUpsideX = targetHexPot.GetMyVector().x - xOffSet;
                        var leftUpsideY = targetHexPot.GetMyVector().y + yExtendedOffSet;
                        var leftUpsidePos = new Vector2(leftUpsideX, leftUpsideY);
                        var leftUpsidePot = listHexPots.FirstOrDefault(p => p.GetMyVector() == leftUpsidePos);
                        if (leftUpsidePot != null)
                        {
                            var leftUpsideHex = leftUpsidePot.myHexagon;
                            
                            if (checkType == "ColorCheck")
                            {
                                if (leftUpsideHex.GetMyColorID() == hexagon.GetMyColorID())
                                {
                                    m_MatchHexagon.Add(leftUpsideHex);
                                }
                            }
                            else if (checkType == "GroupSelect" || checkType == "PotentialCheck")
                            {
                                m_MatchHexagon.Add(leftUpsideHex);   
                                m_BarrierTypeValue = 5;
                            }
                        }
                    }
                }

                for (var l = 0; l < 2; l++)
                {
                    m_CheckNumber[l] += 1;

                    if (m_CheckNumber[l] == 6)
                    {
                        m_CheckNumber[l] = 0;
                    }
                }

                if (m_MatchHexagon.Count >= 2)
                {
                    if (checkType == "ColorCheck")
                    {
                        if (!listMatchHexagons.Contains(hexagon)) listMatchHexagons.Add(hexagon);
                        foreach (var match in m_MatchHexagon)
                        {
                            if (listMatchHexagons.Contains(match)) continue;
                            listMatchHexagons.Add(match);
                        }
                    }
                    else if (checkType == "GroupSelect")
                    {
                        if (!listSelectedPots.Contains(targetHexPot)) listSelectedPots.Add(targetHexPot);
                        
                        foreach (var match in m_MatchHexagon)
                        {
                            if (listSelectedPots.Contains(listHexPots[match.GetMyPotID()])) continue;
                            listSelectedPots.Add(listHexPots[match.GetMyPotID()]);
                        }
                    }
                    else if (checkType == "PotentialCheck")
                    {
                        listPotentials.Clear();
                        
                        m_MainHexagon = hexagon;
                        m_SecondHexagon = m_MatchHexagon[0];
                        m_ThirdHexagon = m_MatchHexagon[1];
                        m_MatchHexagon.Add(m_MainHexagon);
                        
                        MatchPotentialCheck();
                    }

                    m_CheckNumber[0] = 0;
                    m_CheckNumber[1] = 1;
                    break;
                }
            }
        }
    }

}
