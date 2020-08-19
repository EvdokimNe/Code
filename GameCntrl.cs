using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class GameCntrl : MonoBehaviour
{
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private LayerMask _layerMask;
    [SerializeField]
    private Text _score;
    [SerializeField]
    private string _nextScene;

    private List<People> _selectedObjects = new List<People>();
    private List<People> _allObjects = new List<People>();
    private List<GameObject> _coinsLevel = new List<GameObject>();
    
    private bool _canMove = true;
    private bool _win = false;

    private int _localCoins;
    private int _coins;
    
    private void Start()
    {
        AddAllCoins();
        var objects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var gameObject in objects)
        {
           _allObjects.Add(gameObject.GetComponent<People>()); 
        }
        _coinsLevel = GameObject.FindGameObjectsWithTag("Coin").ToList();
    }
    
    private IEnumerator CreateRoadCord(GameObject selectedObj)
    {
        var points = new List<Vector3>();
        points.Add(selectedObj.transform.position);
        People people = selectedObj.GetComponent<People>();
        if (people._finished)
        {
            people.ResetFinished();
            SelectedObjDell(people);
            yield break;
        }
        SelectedObjAdd(people);

        while (true)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, 100f,~_layerMask);
            var pointPosition = hit.point;
            pointPosition.y = 0.02f;
            if ((points[points.Count - 1] - pointPosition).magnitude > 0.05f)
            {
                points.Add(pointPosition);
            }
            if (Input.GetMouseButtonUp(0) || hit.collider == null)
            {
                people.RoadPoints(points);
                SelectedObjStartMove();
                yield break; 
            }
            people.OnDrawLine(points.ToArray());
            yield return null;
        }
    }
    
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, 100f);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                if (CanSelect())
                {
                    StartCoroutine(CreateRoadCord(hit.collider.gameObject));
                }
            }
        }
    }

    private bool CanSelect()
    {
        for (int i = 0; i < _selectedObjects.Count; i++)
        {
            if (_selectedObjects[i]._moveLastPos == false)
            {
                return false;
            }
        }
        return true;
    }

    public void BackUp()
    {
        for (int i = 0; i < _allObjects.Count; i++)
        {
            if (_allObjects[i]._moveLastPos == false)
            {
                return;
            }
        }
        CoinsBack();
        for (int x = 0; x < _allObjects.Count; x++)
        {
            _allObjects[x].ParkedCheck();
        }
    }
    public void CheckWin()
    {
        for (int i = 0; i < _allObjects.Count; i++)
        {
            if (_allObjects[i]._finished == false)
            {
                return;
            }
        }
        _win = true;
        AddAllCoins();
        Invoke("LoadNextSceneMethod",2f);
    }
    
    private void SelectedObjStartMove()
    {
        foreach (var obj in _selectedObjects)
        {
            obj.Moved();
        }
    }
    private void SelectedObjAdd(People selectedObj)
    {
        if(!_selectedObjects.Contains(selectedObj))
        {
            _selectedObjects.Add(selectedObj);
        }
    }
    private void SelectedObjDell(People selectedObj)
    {
        if(_selectedObjects.Contains(selectedObj))
        {
            _selectedObjects.Remove(selectedObj);
        }
    }

    internal void AddLocalCoins()
    {
        _localCoins++;
    }
    private void CoinsBack()
    {
        _localCoins = 0;
        if (!_win)
        {
            foreach (var coins in _coinsLevel)
            {
                coins.transform.position = new Vector3(coins.transform.position.x, 0, coins.transform.position.z);
            }
        }
    }
    private void AddAllCoins()
    {
        _coins += _localCoins;
        var c = _coins.ToString();
        _score.text = c;
    }

    private void LoadNextSceneMethod()
    {
        SceneManager.LoadScene(_nextScene.ToString());
    }
}

