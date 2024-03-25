using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInGame : UIBase
{
    StringBuilder _sb = new StringBuilder();

    [SerializeField] TextMeshProUGUI _levelName;
    [SerializeField] GameObject[] _weapons;
    Image[] _weaponFills;
    [SerializeField]TextMeshProUGUI _ammoTextMesh;

    int _currentWeaponIndex = -1;
    Vector3 _mainWeaponPos;
    int _currentAmmoCount;

    [SerializeField] Image _expMaxImage;
    [SerializeField] Image _expCurrentImage;
    [SerializeField] TextMeshProUGUI _levelText;

    [SerializeField] Image _mapPlayer;
    [SerializeField] Image _mapImage;

    Player _player;

    [SerializeField] MMProgressBar _girlHpBar;
    TextMeshProUGUI _girlHpTextMesh;
    [SerializeField] MMProgressBar _girlMentalBar;
    TextMeshProUGUI _mentalTextMesh;
    [SerializeField] MMProgressBar _creatureHpBar;
    TextMeshProUGUI _creatureHpTextMesh;
    [SerializeField] MMProgressBar _wallHpBar;
    TextMeshProUGUI _wallHpTextMesh;

    [SerializeField] GameObject _wallIndicator;
    [SerializeField] GameObject _wallIndicatorArrow;

    float _maxHpWidth;
    float _maxHpHeight;

    [SerializeField] List<Image> _skillMaskList;
    [SerializeField] List<TextMeshProUGUI> _skillNameList;

    [SerializeField] Image _fadeOut;

    int _prePanicLevel;

    int _preMental;
    public override void Init()
    {
        _isInitDone = true;

        _weaponFills = new Image[_weapons.Length];

        for(int i =0; i < _weapons.Length; i++) 
        {
            _weaponFills[i] = _weapons[i].transform.Find("Fill").GetComponent<Image>();
        }


        StartCoroutine(CorShowLevelName());
        Managers.GetManager<GameManager>().LoadNewSceneHandler += (mapData) =>
        {
            StartCoroutine(CorShowLevelName());
        };

        _girlHpTextMesh = _girlHpBar.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _mentalTextMesh = _girlMentalBar.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _creatureHpTextMesh = _creatureHpBar.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _wallHpTextMesh = _wallHpBar.transform.Find("Text").GetComponent<TextMeshProUGUI>();

    }

    private void Update()
    {
        if (!_isInitDone) return;

        if (Managers.GetManager<GameManager>().PanicLevel != _prePanicLevel)
        {
            StartCoroutine(CorShowPanicLevel());
            _prePanicLevel = Managers.GetManager<GameManager>().PanicLevel;
        }

        _sb.Clear();

        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        if (_player == null) return;

        RefreshWeapon();

        if (_expMaxImage && _expCurrentImage)
        {
            int maxExp = Managers.GetManager<GameManager>().MaxExp; 
            int exp = Managers.GetManager<GameManager>().Exp; 
            _expCurrentImage.rectTransform.sizeDelta
                = new Vector2(_expMaxImage.rectTransform.sizeDelta.x * exp/maxExp, _expMaxImage.rectTransform.sizeDelta.y);

            _levelText.text = Managers.GetManager<GameManager>().Level.ToString();
        }
        HandleBar();
        ShowMap();
        ShowSkill();
        //ShowAbility();
        IndicateWall();
    }

    void RefreshWeapon()
    {
        WeaponSwaper weaponSwaper = _player.WeaponSwaper;

        if (weaponSwaper.CurrentWeapon == null)
        {
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].SetActive(false);
            }
            _ammoTextMesh.gameObject.SetActive(false);
            _currentWeaponIndex = -1;
        }
        else if (_currentWeaponIndex != weaponSwaper.WeaponIndex)
        {
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (i == weaponSwaper.WeaponIndex)
                {
                    _weapons[i].gameObject.SetActive(true);
                }
                else
                {
                    _weapons[i].gameObject.SetActive(false);
                }
            }
            _currentWeaponIndex = weaponSwaper.WeaponIndex;

            _ammoTextMesh.gameObject.SetActive(true);

            if (weaponSwaper.CurrentWeapon != null)
            {
                _weaponFills[_currentWeaponIndex].fillAmount = (float)weaponSwaper.CurrentWeapon.CurrentAmmo / weaponSwaper.CurrentWeapon.MaxAmmo;
                _ammoTextMesh.text = $"{weaponSwaper.CurrentWeapon.CurrentAmmo}/{weaponSwaper.CurrentWeapon.MaxAmmo}";
                _currentAmmoCount = weaponSwaper.CurrentWeapon.CurrentAmmo;
            }
            else
            {
                _weaponFills[_currentWeaponIndex].fillAmount = 0;
                _ammoTextMesh.text = $"0/0";
            }
        }

        if (weaponSwaper.CurrentWeapon != null && _currentAmmoCount != weaponSwaper.CurrentWeapon.CurrentAmmo)
        {
            _weaponFills[_currentWeaponIndex].fillAmount = (float)weaponSwaper.CurrentWeapon.CurrentAmmo / weaponSwaper.CurrentWeapon.MaxAmmo;
            _ammoTextMesh.text = $"{weaponSwaper.CurrentWeapon.CurrentAmmo}/{weaponSwaper.CurrentWeapon.MaxAmmo}";
            _currentAmmoCount = weaponSwaper.CurrentWeapon.CurrentAmmo;
        }
    }

    void HandleBar()
    {
        Character girl = Managers.GetManager<GameManager>().Girl;
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        if (girl)
        {
            _girlHpBar.UpdateBar01((float)girl.Hp / girl.MaxHp);
            _girlHpTextMesh.text = girl.Hp.ToString();

            if ((int)Managers.GetManager<GameManager>().Mental != _preMental)
            {
                _girlMentalBar.UpdateBar01((float)Managers.GetManager<GameManager>().Mental / Managers.GetManager<GameManager>().MaxMental);
                _preMental = (int)Managers.GetManager<GameManager>().Mental;
                _mentalTextMesh.text = _preMental.ToString();
            }
        }
        else
        {
            _girlHpBar.UpdateBar01(0);
            _girlHpTextMesh.text = "0";
        }
        if (wall)
        {
            _wallHpBar.UpdateBar01((float)wall.Hp / wall.MaxHp);
            _wallHpTextMesh.text = wall.Hp.ToString();
        }
        else
        {
            _wallHpBar.UpdateBar01(0);
            _wallHpTextMesh.text = "0";
        }

        if (creature)
        {
            _creatureHpBar.UpdateBar01((float)creature.Hp / creature.MaxHp);
            _creatureHpTextMesh.text = creature.Hp.ToString();
        }
        else
        {
            _creatureHpBar.UpdateBar01(0);
            _creatureHpTextMesh.text = "0";
        }

    }
    void ShowMap()
    {
        if (!Managers.GetManager<GameManager>().Player) return;
        float mapImageSize = _mapImage.rectTransform.sizeDelta.x;
        float mapSize = Managers.GetManager<GameManager>().MapSize;
        float playerPosition = Managers.GetManager<GameManager>().Player.transform.position.x;

        _mapPlayer.transform.localPosition = new Vector3(-mapImageSize / 2 + Mathf.Clamp01(playerPosition / mapSize) * mapImageSize,0,0);
    }

    //void ShowAbility()
    //{
    //    if (_creatureSkillCurtain)
    //    {
    //        CreatureAI creature = Managers.GetManager<GameManager>().CreatureAI;
    //        if (creature.SelectedSpecialAbility != Define.CreatureSkill.None)
    //        {
    //            _creatureSkillCurtain.transform.localPosition =
    //                new Vector3(0,-100 + 100 * creature.SpecialAbilityElaspedTime / creature.SpecialAbilityCoolTime, 0);
    //        }
    //        else
    //        {
    //            _creatureSkillCurtain.transform.localPosition = new Vector3(0, -100, 0);
    //        }
    //    }
    //}

    void ShowSkill()
    {
        List<SkillSlot> skillSlotList = Managers.GetManager<AbilityManager>().SkillSlotList;

        int i = 0;
        for (i = 0; i < skillSlotList.Count; i++)
        {
            if (_skillMaskList.Count <= i) continue;
            if (skillSlotList[i] == null) continue;
            if (skillSlotList[i].skillCoolTime == 0)
                _skillMaskList[i].rectTransform.sizeDelta = new Vector2(100, 100);
            else
                _skillMaskList[i].rectTransform.sizeDelta = new Vector2(100, 100 * (1 - (skillSlotList[i].skillTime / skillSlotList[i].skillCoolTime)));
            if (skillSlotList[i].skillData== null)
                _skillNameList[i].text = string.Empty;
            else
                _skillNameList[i].text = skillSlotList[i].skillData.skillName.ToString();
        }
        for (; i < _skillMaskList.Count; i++)
        {
            _skillMaskList[i].rectTransform.sizeDelta = new Vector2(100, 100);
            _skillNameList[i].text = string.Empty;
        }

    }
    void IndicateWall()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(Managers.GetManager<GameManager>().Player.transform.position);
        Vector3 wallPosition = Camera.main.WorldToScreenPoint(Managers.GetManager<GameManager>().Wall.transform.position);
        playerPosition.z = 0;
        wallPosition.z = 0;

        Vector3 direction = wallPosition - playerPosition;
        Vector3 wallToCanvasDistance = wallPosition - transform.position;

        wallToCanvasDistance.x -= screenWidth / 2;
        wallToCanvasDistance.y -= screenHeight / 2;

        if (Mathf.Abs(wallToCanvasDistance.x) < screenWidth / 2 + 100 && Mathf.Abs(wallToCanvasDistance.y)+100 < screenHeight / 2 + 100)
        {
            _wallIndicator.SetActive(false);
            return;
        }
        _wallIndicator.SetActive(true);

        float angle  = Mathf.Atan2(direction.y, direction.x);

        Vector3 distanceCanvasToPlayerPosition = playerPosition - transform.position;
        
        distanceCanvasToPlayerPosition.x -= screenWidth / 2;
        distanceCanvasToPlayerPosition.y -= screenHeight/ 2;

        float paddingX = 80 * Mathf.Cos(angle);
        float paddingY = 80 * Mathf.Sin(angle);

        if (_wallIndicatorArrow)
            _wallIndicatorArrow.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg+90);

        float x = 0;
        float y = 0;

        float distanceX = wallPosition.x < 0 ? Mathf.Abs(wallPosition.x) : wallPosition.x - screenWidth;
        float distanceY = wallPosition.y < 0 ? Mathf.Abs(wallPosition.y) : wallPosition.y - screenWidth;

        if (wallPosition.x < screenWidth && wallPosition.x > 0)
        {
            if (wallPosition.y > screenHeight || wallPosition.y < 0)
            {
                y = (direction.y > 0 ? screenHeight / 2 : -screenHeight / 2) - paddingY;
                x = (((direction.y > 0 ? screenHeight / 2 - distanceCanvasToPlayerPosition.y : -screenHeight / 2 - distanceCanvasToPlayerPosition.y) - paddingY)/ Mathf.Tan(angle)
                    + distanceCanvasToPlayerPosition.x);
                if (x - paddingX < transform.position.x - screenWidth/2)
                    x -= paddingX;
                if (x - paddingX > transform.position.x + screenWidth / 2)
                    x -= paddingX;
            }
        }
        else if (wallPosition.y <= screenHeight && wallPosition.y > 0)
        {
            if (wallPosition.x > screenWidth || wallPosition.x < 0)
            {

                x = (direction.x > 0 ? screenWidth / 2 : -screenWidth / 2) - paddingX;
                y = (Mathf.Tan(angle) * (direction.x > 0 ? screenWidth / 2 - distanceCanvasToPlayerPosition.x : -screenWidth / 2 - distanceCanvasToPlayerPosition.x)
                    + distanceCanvasToPlayerPosition.y) - paddingY;
            }
        }
        else
        {
            if(distanceX > distanceY)
            {
                x = (direction.x > 0 ? screenWidth / 2 : -screenWidth / 2) - paddingX;
                y = (Mathf.Tan(angle) * (direction.x > 0 ? screenWidth / 2 - distanceCanvasToPlayerPosition.x : -screenWidth / 2 - distanceCanvasToPlayerPosition.x)
                    + distanceCanvasToPlayerPosition.y) - paddingY;
            }
            else
            {
                y = (direction.y > 0 ? screenHeight / 2 : -screenHeight / 2) - paddingY;
                x = ((direction.y > 0 ? screenHeight / 2 - distanceCanvasToPlayerPosition.y : -screenHeight / 2 - distanceCanvasToPlayerPosition.y) / Mathf.Tan(angle)
                    + distanceCanvasToPlayerPosition.x) - paddingX;

                if (x - paddingX < transform.position.x - screenWidth / 2)
                    x -= paddingX;
                if (x - paddingX > transform.position.x + screenWidth / 2)
                    x -= paddingX;
            }
        }
        if(y < -screenHeight/2 - paddingY)
            y= -screenHeight/2 - paddingY;
        else if( y > screenHeight /2 - paddingY)
            y= screenHeight/2 - paddingY;


        Vector3 indicatorPosition = new Vector3(x, y, 0);

        if (_wallIndicator)
        {
            _wallIndicator.transform.localPosition = indicatorPosition;
        }
    }

    public override void Open(bool except)
    {
        
    }
    public override void Close(bool except)
    {
        
    }
    IEnumerator CorShowPanicLevel()
    {
        float alpha = 0;
        Color color = new Color(1, 1, 1, alpha);
        _levelName.color = color;
        if (_levelName)
        {
            _levelName.text = $"패닉 { Managers.GetManager<GameManager>().PanicLevel}단계";
            while (alpha < 1)
            {
                color = new Color(1, 1, 1, alpha);
                alpha += Time.deltaTime / 3;

                _levelName.color = color;
                yield return null;
            }
            alpha = 1;

            yield return new WaitForSeconds(1);


            while (alpha > 0)
            {
                color = new Color(1, 1, 1, alpha);
                alpha -= Time.deltaTime / 3;

                _levelName.color = color;
                yield return null;
            }
        }
    }


    IEnumerator CorShowLevelName()
    {
        float alpha = 0;
        Color color = new Color(1, 1, 1, alpha);
        _levelName.color = color;
        if (_levelName)
        {
            _levelName.text = Managers.GetManager<GameManager>().LevelName;
            while (alpha < 1)
            {
                color = new Color(1, 1, 1, alpha);
                alpha += Time.deltaTime / 3;

                _levelName.color = color;
                yield return null;
            }
            alpha = 1;

            yield return new WaitForSeconds(1);


            while (alpha > 0)
            {
                color = new Color(1, 1, 1, alpha);
                alpha -= Time.deltaTime / 3;

                _levelName.color = color;
                yield return null;
            }
        }
    }
   

    public void LoadSceneFadeOut()
    {
        StartCoroutine(CorLoadSceneFadeOut());
    }
    IEnumerator CorLoadSceneFadeOut()
    {
        Color color = new Color(0, 0, 0, 0);
        _fadeOut.color = color;

        while (true) 
        { 
            color.a += Time.deltaTime/2;
            _fadeOut.color = color;
            if (color.a >= 1)
                break;
            yield return null;
        }


        while (true)
        {
            if (Managers.GetManager<GameManager>().IsLoadEnd)
                break;

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            color.a -= Time.deltaTime / 2;
            _fadeOut.color = color;
            if (color.a <= 0)
                break;
            yield return null;
        }
    }
}
