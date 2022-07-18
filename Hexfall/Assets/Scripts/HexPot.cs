using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexPot : MonoBehaviour
{
    public int potID;
    public Hexagon myHexagon;
    public bool nullCheck;
    
    private SpriteRenderer m_SpriteRenderer;
    
    private float m_XPos, m_YPos;
    private bool m_LerpState;
    private bool m_YExtendedOffSetCheck;
    private void Start() { m_SpriteRenderer = GetComponent<SpriteRenderer>(); }
    public void SetInitializePos(float targetPosX, float targetPosY, bool offSetCheck)
    {
        m_YExtendedOffSetCheck = offSetCheck;
        m_XPos = targetPosX;
        m_YPos = targetPosY + ExtendedYOffSet();

        var myTransform = transform;
        myTransform.position = new Vector3(m_XPos, m_YPos, 0.05f);
        myTransform.gameObject.name = $"POD ID : {HexagonManager.Instance.listHexPots.IndexOf(this)}";
    }
    public void SelectedColor(bool selectType)
    {
        if (selectType)
        {
            m_SpriteRenderer.color = new Color(1,1,1,0.05f);
            return;
        }
        
        m_SpriteRenderer.color = new Color(1,1,1,0);
    }
    public Vector2 GetMyVector()
    {
        return new Vector2(m_XPos, m_YPos);
    }
    private float ExtendedYOffSet()
    {
        if(m_YExtendedOffSetCheck)
        {
            return 0;
        }
        
        return HexagonManager.Instance.yExtendedOffSet;
    }
    public void OnMouseDown()
    {
        if (HexagonManager.Instance.GameOver) return;
        
        var listSelectedPots = HexagonManager.Instance.listSelectedPots;

        if (listSelectedPots.Contains(this)) // Spinning when you tap second time.
        {
            var totalX = 0f;
            var totalY = 0f;
            
            foreach(var selected in listSelectedPots) 
            { 
                totalX += selected.GetMyVector().x;
                totalY += selected.GetMyVector().y;
            }
            
            var centerX = totalX / listSelectedPots.Count;
            var centerY = totalY / listSelectedPots.Count;

            if (centerX <= GetMyVector().x) {HexagonManager.Instance.ClockWise(true);  }
            else if (centerX > GetMyVector().x) { HexagonManager.Instance.ClockWise(false); }
        }
        
        else
        {
            if (HexagonManager.Instance.SwipeProtect || HexagonManager.Instance.ExplodeProtect) return; // Multiple click protect.
            

            if (HexagonManager.Instance.listPreviousSelectedHexagons.Count > 2 & HexagonManager.Instance.listMatchHexagons.Count <= 0) // The previous group returns to its initial position.
            {
                for (int i = 0; i < HexagonManager.Instance.listPreviousSelectedHexagons.Count; i++)
                {
                    HexagonManager.Instance.listPreviousSelectedHexagons[i].SetPotData(HexagonManager.Instance.listHexPots
                        [HexagonManager.Instance.listSelectedPotIDs[i]], false); 
                    
                    HexagonManager.Instance.listPreviousSelectedHexagons[i].LerpStateCheck();
                }
            }

            HexagonManager.Instance.BoostDeActivate();

            for (int j = 0; j < 6; j++)
            {
                HexagonManager.Instance.MatchCheck("GroupSelect", false, this,HexagonManager.Instance.listHexPots);

                HexagonManager.Instance.listSelectedPotIDs.Clear();
                HexagonManager.Instance.listPreviousSelectedHexagons.Clear();

                foreach (var selected in listSelectedPots)
                    HexagonManager.Instance.listSelectedPotIDs.Add(selected.potID);

                for (var i = 0; i < HexagonManager.Instance.listSelectedPotIDs.Count; i++)
                    HexagonManager.Instance.listPreviousSelectedHexagons.Add(
                        HexagonManager.Instance.listHexPots[
                            HexagonManager.Instance.listSelectedPotIDs[i]].myHexagon);

                HexagonManager.Instance.BoostActivate();
                return;
            }
        }
    }
}