using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInGame : UIBase
{
    StringBuilder _sb = new StringBuilder();
    [SerializeField]TextMeshProUGUI _characterStateText;

    [SerializeField] Image[] _weaponImage;

    int _currentWeaponIndex = -1;
    Vector3 _mainWeaponPos;

    [SerializeField] Image _expMaxImage;
    [SerializeField] Image _expCurrentImage;
    [SerializeField] TextMeshProUGUI _levelText;

    [SerializeField] Image _mapPlayer;
    [SerializeField] Image _mapImage;

    Player _player;

    [SerializeField] MMProgressBar _daughterHpBar;
    [SerializeField] MMProgressBar _fatherHpBar;
    [SerializeField] MMProgressBar _dogHpBar;

    float _maxHpWidth;
    float _maxHpHeight;

    public override void Init()
    {
        _isInitDone = true;
        _mainWeaponPos = _weaponImage[0].transform.localPosition;
    }

    private void Update()
    {
        if (!_isInitDone) return;

       
        _sb.Clear();
        Character character = _player.Character;
        WeaponSwaper weaponSwaper = _player.WeaponSwaper;
        if(character)
        {
            _sb.Append($"체력: {character.MaxHp}\n");
        }
        if(weaponSwaper.CurrentWeapon)
        {
            _sb.Append($"공격력 : {weaponSwaper.CurrentWeapon.Damage}\n");
            _sb.Append($"넉백 : {weaponSwaper.CurrentWeapon.Power}\n");
            _sb.Append($"총알속도 : {weaponSwaper.CurrentWeapon.BulletSpeed}\n");
            _sb.Append($"관통력 : {weaponSwaper.CurrentWeapon.PenerstratingPower}\n");
            _sb.Append($"발사 딜레이 : {weaponSwaper.CurrentWeapon.FireDelay}\n");
            _sb.Append($"리로드속도 : {weaponSwaper.CurrentWeapon.ReloadDelay}\n");
            _sb.Append($"총알 :{weaponSwaper.CurrentWeapon.CurrentAmmo}/{weaponSwaper.CurrentWeapon.MaxAmmo}\n");
        }
        if(_characterStateText)
        {
            _characterStateText.text = _sb.ToString();
        }

        if (_currentWeaponIndex != weaponSwaper.WeaponIndex)
        {
            for(int i =0; i < _weaponImage.Length; i++)
            {
                if (i == weaponSwaper.WeaponIndex)
                {
                    _weaponImage[i].transform.localPosition = _mainWeaponPos;
                    _weaponImage[i].gameObject.SetActive(true);
                }
                else
                {
                    _weaponImage[i].gameObject.SetActive(false);
                }
            }
            _currentWeaponIndex = weaponSwaper.WeaponIndex;
        }

        if(_expMaxImage && _expCurrentImage)
        {
            int maxExp = Managers.GetManager<GameManager>().MaxExp; 
            int exp = Managers.GetManager<GameManager>().Exp; 
            _expCurrentImage.rectTransform.sizeDelta
                = new Vector2(_expMaxImage.rectTransform.sizeDelta.x * exp/maxExp, _expMaxImage.rectTransform.sizeDelta.y);

            _levelText.text = Managers.GetManager<GameManager>().Level.ToString();
        }
        HandleHp();
        ShowMap();
    }

    void HandleHp()
    {
        Character daughter = Managers.GetManager<GameManager>().Daughter;
        Character dog = Managers.GetManager<GameManager>().Dog;
        Character father = Managers.GetManager<GameManager>().Father;

        if(daughter)
            _daughterHpBar.UpdateBar01((float)daughter.Hp/ daughter.MaxHp);
        else
            _daughterHpBar.UpdateBar01(0);
        if (dog)
            _dogHpBar.UpdateBar01((float)dog.Hp / dog.MaxHp);
        else
            _dogHpBar.UpdateBar01(0);

        if (father)
            _fatherHpBar.UpdateBar01((float)father.Hp / father.MaxHp); 
        else
            _fatherHpBar.UpdateBar01(0);
    }
    void ShowMap()
    {
        if (!Managers.GetManager<GameManager>().Player) return;
        float mapImageSize = _mapImage.rectTransform.sizeDelta.x;
        float mapSize = Managers.GetManager<GameManager>().MapSize;
        float playerPosition = Managers.GetManager<GameManager>().Player.transform.position.x;

        _mapPlayer.transform.localPosition = new Vector3(-mapImageSize / 2 + Mathf.Clamp01(playerPosition / mapSize) * mapImageSize,0,0);
    }

    public void SetPlayerCharacter(Player player)
    {
        _player= player;
    }

    public override void Open()
    {
        
    }
    public override void Close()
    {
        
    }

    IEnumerator CorChangeWeapon(int index)
    {
        for(int i =1; i <= 60; i++)
        {
            if(index == 0)
            {
                float moveVel = 100 / 30f;
                if (i < 30)
                {
                    _weaponImage[index].transform.position = new Vector3(_mainWeaponPos.x - moveVel * Time.deltaTime, _weaponImage[index].transform.position.y);
                }
                else
                {
                    _weaponImage[index].transform.position = new Vector3(_mainWeaponPos.x + moveVel * Time.deltaTime, _weaponImage[index].transform.position.y);
                }
                if(i == 30)
                {
                    _weaponImage[index].transform.SetSiblingIndex(2);
                    _weaponImage[_currentWeaponIndex].transform.SetSiblingIndex(1);
                    _weaponImage[3 - index - _currentWeaponIndex].transform.SetSiblingIndex(0);
                }
                _weaponImage[_currentWeaponIndex].transform.position = new Vector3(_mainWeaponPos.x - (20/60f * Time.deltaTime), _weaponImage[_currentWeaponIndex].transform.position.y);
                _weaponImage[3 - index - _currentWeaponIndex].transform.position = new Vector3(_mainWeaponPos.x - (40/60f * Time.deltaTime), _weaponImage[3 - index - _currentWeaponIndex].transform.position.y);
            }

            yield return null;
        }

        _weaponImage[index].transform.position = _mainWeaponPos;
        _weaponImage[_currentWeaponIndex].transform.position = _mainWeaponPos - Vector3.right * 20f;
        _weaponImage[3 - index - _currentWeaponIndex].transform.position = _mainWeaponPos - Vector3.right * 40f;
        _currentWeaponIndex = index;
    }
}
