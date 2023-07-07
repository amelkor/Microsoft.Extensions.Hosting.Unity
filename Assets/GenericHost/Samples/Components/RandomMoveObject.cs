using UnityEngine;

namespace GenericHost.Samples
{
    public class RandomMoveObject : MonoBehaviour
    {
        public float speed = 1f;
        public float distance = 3f;

        private Vector3 _startPos;
        private Vector3 _targetPos;

        private void Start()
        {
            _startPos = transform.position;
            SetNewTarget();
        }
        
        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, _targetPos) < 0.01f)
            {
                SetNewTarget();
            }
        }
        
        private void SetNewTarget()
        {
            var randomX = Random.Range(-distance, distance);
            var randomY = Random.Range(-distance, distance);
            
            _targetPos = new Vector3(_startPos.x + randomX, _startPos.y + randomY, _startPos.z);
        }
    }
}