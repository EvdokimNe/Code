using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class People : MonoBehaviour
{
    [SerializeField] private GameCntrl _GK;
    [SerializeField] private GameObject _finalPoint;
    [GradientUsage(true)]
    [SerializeField] private Gradient _gradient;
    [SerializeField] private LineRenderer _line;
    
    [SerializeField] private float _speed;
    [SerializeField] private Animator _animator;
   
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    internal bool _finished;
    public bool _moveLastPos = true;
    
    private List<Vector3> _road = new List<Vector3>();
    private int _roadPoint = 0;


    public void Start()
    {
        _moveLastPos = true;
        _line.enabled = false;
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _line.colorGradient = _gradient;
    }

    private IEnumerator MoveToRoad()
    {
        _moveLastPos = false;
        _finished = false;
        transform.position = _startPosition;
        _animator.SetTrigger("Run");
        while (true)
        {
            if (_animator.GetBool("Dead") == true)
            {
                yield break;
            }
            transform.LookAt(_road[_roadPoint]);
            transform.position = Vector3.MoveTowards(transform.position, _road[_roadPoint], _speed * Time.deltaTime);
            if (_roadPoint == _road.Count - 1)
            {
                _moveLastPos = true;
                Invoke("BackUpThis",0.5f);
                _animator.SetTrigger("Stop");
                _roadPoint = 0;
                yield break;
            }

            if (Vector3.Distance(_road[_roadPoint], transform.position) < 0.01f)
            {
                _roadPoint++;
            }

            yield return null;
        }
        
    }

    private void BackUpThis()
    {
        _animator.SetTrigger("Stop");
        _GK.BackUp();
    }  
        
    public void OnDrawLine(Vector3[] point)
    {
        _line.enabled = true;
        _line.positionCount = point.Length;
        _line.SetPositions(point);
    }

    public void RoadPoints(List<Vector3> point)
    {
        _road = point;
    }

    public void Moved()
    {
        StartCoroutine(MoveToRoad());
    }

    public void ParkedCheck()
    {
        _moveLastPos = true;
        if (!_finished)
        {
            transform.position = _startPosition;
            transform.rotation = _startRotation;
        }
    }

    public void ResetFinished()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _line.enabled = false;
        _finished = false; 
        _moveLastPos = true;
        _animator.SetTrigger("Stop");
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _finished = false;
            _moveLastPos = true;
            collision.gameObject.GetComponent<People>()._moveLastPos = true;
            _animator.SetBool("Dead",true);
            _roadPoint = 0;
            Invoke("BackUpThis", 3.5f);
        }
    }
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Coin"))
        {
            _GK.AddLocalCoins();
            coll.gameObject.transform.position = new Vector3(coll.gameObject.transform.position.x,100,coll.gameObject.transform.position.z);
        }
    } 
    void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject == _finalPoint && _moveLastPos && !_finished)
        {
            _animator.SetTrigger("Finish");
            _finished = true;
            _GK.CheckWin();
        }
    } 
}

