using System;
using TMPro;
using UnityEngine;


public class Hexagon : Data
{
    [SerializeField] 
    private SpriteRenderer[] spriteModels;

    [SerializeField]
    private TMP_Text countDownText;
    
    public int explodeCountDown;
    private HexPot m_MyHexPot;
    private int m_MyHexPotID;
    private int m_ColorID;
    private float m_TargetX, m_TargetY, m_TargetZ;
    private bool m_LerpState;
    private int m_LerpSpeed;
    public bool BombActive { get; private set; }
    private void Update()
    {
        if (m_LerpState)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(m_TargetX, m_TargetY, m_TargetZ),
                 Time.deltaTime * m_LerpSpeed);
            
            LerpStateCheck();
        }
        
    }

    public void LerpStateCheck()
    {
        if (HexagonManager.Instance.GameOver) return;
        if (Vector3.Distance(transform.position, new Vector3(m_TargetX, m_TargetY, 0f)) >= 0.2f)
        {
            m_LerpState = true;
        }
        else
        {
            m_LerpState = false;
            transform.position = new Vector3(m_TargetX, m_TargetY, 0);
        }
    }
    public void SetMyColor(int ID)
    {
        m_ColorID = ID;

        foreach (var model in spriteModels)
        {
            model.color = HexagonManager.Instance.hexagonColors[m_ColorID];
        }
    }
    public void SetPotData(HexPot newHexPot, bool startInitializeCheck)
    {
        m_MyHexPot = newHexPot;
        m_MyHexPotID = newHexPot.potID;
        newHexPot.myHexagon = this;

        SetLerpPos(startInitializeCheck);
    }
    private void SetLerpPos(bool initializeCheck)
    {
        m_TargetX = m_MyHexPot.GetMyVector().x;
        m_TargetY = m_MyHexPot.GetMyVector().y;
        m_TargetZ = -0.05f;
        
        transform.gameObject.name = $"Hexagon || ID : {m_MyHexPot.potID}";
        
        if (!initializeCheck)
        {
            m_LerpSpeed = 18;
            LerpStateCheck();
            return;
        }

        m_LerpSpeed = 15;
        transform.position = new Vector3(m_TargetX, 45, -0.1f);
    }
    public void HexagonTypeChange(string spriteType)
    {
        foreach (var model in spriteModels) { model.gameObject.SetActive(false); }
        var bombList = HexagonManager.Instance.listBombHexagons;


        if (spriteType == "Hexagon")
        {
            spriteModels[0].gameObject.SetActive(true);
            if (bombList.Contains(this)) bombList.Remove(this);
            BombActive = false;
        }
        else if (spriteType == "Bomb")
        {
            spriteModels[1].gameObject.SetActive(true);
            if (!bombList.Contains(this)) bombList.Add(this);
            BombActive = true;
        }
    }
    public void CountdownSet(int additionalValue)
    {
        explodeCountDown += additionalValue;
        explodeCountDown = Mathf.Clamp(explodeCountDown, -1, 99);
        if(explodeCountDown <= 0) HexagonManager.Instance.GameOverCall();
        
        countDownText.SetText(explodeCountDown.ToString());
    }

    #region Getter

    public int GetMyPotID()
    {
        return m_MyHexPotID;
    }

    public int GetMyColorID()
    {
        return m_ColorID;
    }

    #endregion
}
