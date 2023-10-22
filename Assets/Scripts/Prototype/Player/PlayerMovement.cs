using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ProgrammingTask.NetPlayer
{
    /// <summary>
    /// Player movement logic
    /// This is not using server side
    /// We trust client to do its own logic
    /// Maybe server is better but not sure if it is required in this project
    /// </summary>
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Camera")] 
        [SerializeField] private CinemachineFreeLook _virtualCamera;
        [SerializeField] private Transform _cameraMainTransform;
        
        [Header(("Character Settings"))]
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Vector3 _playerVelocity;
        [SerializeField] private bool _groundedPlayer;
        [SerializeField] private float _playerSpeed = 5.0f;
        [SerializeField] private float _jumpHeight = 1.0f;
        [SerializeField] private float _gravityValue = -9.81f;
        [SerializeField] private float _rotationSpeed = 15f;
        
        [Header("Player Input Actions")] 
        [SerializeField] private InputActionReference _movementControl;
        [SerializeField] private InputActionReference _jumpControl;
        
        private void OnDisable()
        {
            if (!IsOwner)
                return;
            
            _movementControl.action.Disable();
            _jumpControl.action.Disable();
        }

        /// <summary>
        /// Setup local player
        /// </summary>
        public void SetupPlayer()
        {
            _virtualCamera = FindObjectOfType<CinemachineFreeLook>();
            _virtualCamera.Follow = transform;
            _virtualCamera.LookAt = transform;
            _cameraMainTransform = Camera.main.transform;
            
            _movementControl.action.Enable();
            _jumpControl.action.Enable();
        }

        private void Update()
        {
            if(!IsLocalPlayer)
                return;
            
            //! For some reason, changing scene is quite slow.
            //! This is work around... for now...
            if(_cameraMainTransform == null)
                return;
            
            //! Stop y axis from doing weird calculation
            _groundedPlayer = _controller.isGrounded;
            if (_groundedPlayer && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }
            
            //! movement
            Vector2 movement = _movementControl.action.ReadValue<Vector2>();
            Vector3 move = new Vector3(movement.x, 0, movement.y);
            move = _cameraMainTransform.forward * move.z + _cameraMainTransform.right * move.x;
            move.y = 0f;
            _controller.Move(move * (Time.deltaTime * _playerSpeed));
            
            if (_jumpControl.action.triggered && _groundedPlayer)
            {
                _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
            }

            _playerVelocity.y += _gravityValue * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
            
            //! Rotation
            if (movement != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + _cameraMainTransform.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(0f, targetAngle ,0f);
                
                transform.rotation=  Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * _rotationSpeed);
            }
        }
    }
}