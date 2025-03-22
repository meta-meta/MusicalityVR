using MH.WaterCausticsModules;
using UnityEngine;

namespace Musicality
{
    public class CausticsController : MonoBehaviour
    {
        [SerializeField] private FloatPicker density;
        [SerializeField] private FloatPicker height;
        [SerializeField] private FloatPicker speed;
        [SerializeField] private FloatPicker flow;
        [SerializeField] private FloatPicker direction;

        [SerializeField] private FloatPicker densityW1;
        [SerializeField] private FloatPicker heightW1;
        [SerializeField] private FloatPicker fluctuationW1;
        [SerializeField] private FloatPicker flowW1;
        [SerializeField] private FloatPicker directionW1;
      
        [SerializeField] private FloatPicker densityW2;
        [SerializeField] private FloatPicker heightW2;
        [SerializeField] private FloatPicker fluctuationW2;
        [SerializeField] private FloatPicker flowW2;
        [SerializeField] private FloatPicker directionW2;
        
        [SerializeField] private WaterCausticsTexGenerator _texGen;

        
        private void Awake()
        {
            
        }
    }
}