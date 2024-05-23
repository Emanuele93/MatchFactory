using UnityEngine;

namespace Services
{
    // It should be an interface - I used the abstract class because it is serializable
    public abstract class Service: MonoBehaviour
    {
        internal abstract void Init();
    }
    
    public class ServicesInitializer : MonoBehaviour
    {
        [SerializeField] private Service[] services;

        private void OnEnable()
        {
            foreach (var service in services)
                service.Init();
            
            SoundsManager.PlayMusic();
        }
    }
}
