﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Musicality;
using UnityEngine;

public class Manipulate : MonoBehaviour
{
    public bool IsStretching;// { get; private set; }
    public bool IsGrabbing;// { get; private set; }
    public bool IsTouching;// { get; private set; }
    public bool IsKinematic;// { get; private set; }
    public bool UseGravity;// { get; private set; }

    public bool ConstrainRoll;
    public bool ControlParent; //TODO use to manipulate parent GameObject instead of this

    private GameObject _touchControllerGrab;
    private MalletHead _malletHeadGrab;
    private OVRInput.Controller _controllerMaskGrab = OVRInput.Controller.None;
    private Transform _parentAtGrab;
    private Rigidbody _rigidbody;
    
    
    private float _controllerDistanceAtPinchStart;
    private GameObject _touchControllerStretch;
    private OVRInput.Controller _controllerMaskStretch = OVRInput.Controller.None;
    private Vector3 _scaleAtStretchStart;

    private Transform _parentAtStart;
    
    private Save _save;

    void Start()
    {
        _save = GameObject.Find("App").GetComponent<Save>();
        _parentAtStart = transform.parent;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        IsKinematic = _rigidbody == null ? true : _rigidbody.isKinematic;
        UseGravity = _rigidbody == null ? false : _rigidbody.useGravity;
    }

    // Update is called once per frame
    void Update()
    {
        if (
            IsTouching &&
            _controllerMaskGrab != OVRInput.Controller.None &&
            OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, _controllerMaskGrab)
        )
        {// Grab start
            IsGrabbing = true;
            _parentAtGrab = transform.parent;
            transform.SetParent(_touchControllerGrab.transform, true);
            
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
        }

        
        if (IsTouching && OVRInput.GetDown(OVRInput.Button.One, _controllerMaskGrab))
        {// Touch and button 1 to save position
            Vibe(0.5f, _controllerMaskGrab);
            _save.SavePosition(gameObject);
        }
        
        if (IsTouching && OVRInput.GetDown(OVRInput.Button.Two, _controllerMaskGrab))
        {// Touch and button 2 to delete saved position
            Vibe(1f, _controllerMaskGrab);
            _save.DeletePosition(gameObject);
        }
        
       

        if (IsStretching)
        {// is Stretching
            // Debug.Log("g: " + _touchControllerGrab.name + " s: " + _touchControllerStretch.name);
            var mag = (_touchControllerGrab.transform.position - _touchControllerStretch.transform.position)
                .magnitude / _controllerDistanceAtPinchStart;

            if (mag != null)
            {
                transform.localScale = _scaleAtStretchStart * mag;
            }

            Vibe(0.2f, _controllerMaskGrab);
            Vibe(0.2f, _controllerMaskStretch);
            
            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, _controllerMaskStretch)
                || OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, _controllerMaskGrab))
            {// Stretch end
                _controllerMaskStretch = OVRInput.Controller.None;
                IsStretching = false;
            }
        }
        
        if (IsGrabbing)
        {// is Grabbing
            if (ConstrainRoll)
            {// lock Z-axis
                transform.rotation = Quaternion.LookRotation(_touchControllerGrab.transform.forward, Vector3.up);
            }
            
            if (
                _controllerMaskStretch != OVRInput.Controller.None &&
                OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, _controllerMaskStretch)
            )
            {// Stretch start
                IsStretching = true;
                Vibe(0.5f, _controllerMaskGrab);
                Vibe(0.5f, _controllerMaskStretch);
                _scaleAtStretchStart = transform.localScale;
                _controllerDistanceAtPinchStart = (_touchControllerGrab.transform.position - _touchControllerStretch.transform.position).magnitude;
            }
            
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, _controllerMaskGrab))
            {// Grab and index trigger to possess/dispossess from LocalAvatar/base
                if (_parentAtGrab == _parentAtStart)
                {
                    transform.SetParent(GameObject.Find("LocalAvatar/base").transform, true);
                }
                else
                {
                    transform.SetParent(_parentAtStart, true);
                }
                
                // TODO: how much of this is necessary? Encapsulate?
                IsGrabbing = false;
                IsStretching = false;
                IsTouching = false;
                _touchControllerGrab = null;
                _malletHeadGrab = null;
                _controllerMaskGrab = OVRInput.Controller.None;
                _controllerMaskStretch = OVRInput.Controller.None;
                _parentAtGrab = null;
            }
            
            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, _controllerMaskGrab))
            {// Grab end
                transform.SetParent(_parentAtGrab, true);
                IsGrabbing = false;
                IsStretching = false;
                IsTouching = false;
                
                // Debug.Log(_pointerGrab.velocityVec);
                
                if (_rigidbody != null)
                {
                    if (!IsKinematic)
                    {
                        _rigidbody.isKinematic = false;
                        _rigidbody.velocity = _malletHeadGrab.velocityVec;
                    }
                    _rigidbody.useGravity = UseGravity;
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var pointer = other.GetComponent<MalletHead>();
        if (!IsTouching && !IsStretching && !IsGrabbing && pointer != null)
        { // Touch start; Grab is possible
            IsTouching = true;
            _malletHeadGrab = pointer;
            _touchControllerGrab = other.gameObject;
            _controllerMaskGrab = pointer.Controller;
            Vibe(0.2f, _controllerMaskGrab);
        } 
        
        if (IsGrabbing && !IsStretching && pointer != null && pointer != _malletHeadGrab)
        { // Stretch is possible
            _controllerMaskStretch = pointer.Controller;
            _touchControllerStretch = other.gameObject;
            Vibe(0.2f, _controllerMaskStretch);
        }
    }

    void Vibe(float strength, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(0.1f, strength, controller);
        StartCoroutine(VibeOff(0.01f, controller));
    }

    IEnumerator VibeOff(float timeout, OVRInput.Controller controller)
    {
        while (true)
        {
            yield return null;
            timeout -= Time.deltaTime;
            if (timeout <= 0f)
            {
                OVRInput.SetControllerVibration(0, 0, controller);
                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTouching && other.gameObject == _touchControllerGrab)
        {
            IsTouching = false;

            if (!IsGrabbing) // Only necessary because OnTriggerExit can be called between frames
            {
                _touchControllerGrab = null;
                _malletHeadGrab = null;
                _controllerMaskGrab = OVRInput.Controller.None;

                var pointer = other.GetComponent<MalletHead>();
                Vibe(0.1f, pointer.Controller);
            }
        }
    }
}