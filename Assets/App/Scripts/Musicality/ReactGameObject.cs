using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Musicality
{
    public class ReactGameObject : MonoBehaviour, IReactComponent<ReactGameObject.State>
    {
        
        public interface ISerializableComponent
        {
            public Object Serialize();
        }
        
        public struct SerializableTransform : ISerializableComponent
        {
            public Quaternion LocalRotation;
            public Vector3 LocalPosition;
            public Vector3 LocalScale;


            public Object Serialize(/* want to take transform here */) 
            {
                throw new NotImplementedException();
            }
        }
        
        public struct State
        {
            public SerializableTransform Transform;
            // public List<Object> Components; // TODO: crunch through these and pass to JsonConvert.Serialize
            public List<String> Components;
        }

        // GameObject IReactComponent<State>.GameObject => gameObject;  // TODO: this was auto generated. what's the value add?
        public GameObject GameObject => gameObject;

        public State CurrentState
        {
            get
            {
                var components = GetComponents<Component>()
                    .Where(cmp => cmp.GetType() != typeof(ReactGameObject))
                    .Select(cmp => cmp switch
                    {
                        // Transform tr => new SerializableTransform() // TODO: List<Object>
                        // {
                        //     LocalPosition = tr.localPosition,
                        //     LocalRotation = tr.localRotation,
                        //     LocalScale = tr.localScale,
                        // },
                        Transform tr => JsonConvert.SerializeObject(new SerializableTransform()
                        {
                            LocalPosition = tr.localPosition,
                            LocalRotation = tr.localRotation,
                            LocalScale = tr.localScale,
                        }),
                        _ => $"{cmp.GetType().Name} not implemented",
                    })
                    .ToList();
                
                
                
                return new State()
                {
                    Transform = new SerializableTransform()
                    {
                        LocalPosition = transform.localPosition,
                        LocalRotation = transform.localRotation,
                        LocalScale = transform.localScale,
                    },
                    Components = components,
                };
            }
        }

        public int ComponentId { get; set; }
        public ReactComponentCollection<State> ComponentCollection { get; set; }

        public void Init(State initialState)
        {
            
        }

        public void Unmount()
        {
        }

        public void UpdateFromState(State nextState)
        {
            
        }

        private void OnValidate()
        {
            Debug.Log(JsonConvert.SerializeObject(CurrentState));
        }
    }
}