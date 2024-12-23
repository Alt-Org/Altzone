using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using ExitGames.Client.Photon;
using MenuUi.Scripts.SwipeNavigation;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.CharacterGallery
{
    public class DraggableCharacter : MonoBehaviour, IGalleryCharacterData, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundSpriteImage;
        [SerializeField] private TextMeshProUGUI _characterNameText;

        private CharacterID _id;

        private Button button;
        private ColorBlock originalColors;

        [HideInInspector] public Transform parentAfterDrag;
        private Transform previousParent;

        public Transform allowedSlot;
        public Transform initialSlot;

        [SerializeField] private ModelView _modelView;

        public delegate void ParentChangedEventHandler(Transform newParent);
        public event ParentChangedEventHandler OnParentChanged;

        [SerializeField]
        private SwipeBlockType _blockType = SwipeBlockType.All;
        [SerializeField]
        private SwipeUI _swipe;
        public int characterTextCounter;

        public CharacterID Id { get => _id; }

        private void Start()
        {
            button = GetComponent<Button>();
            originalColors = button.colors;
            _swipe = transform.root.Find("MainMenuViewSwipe").Find("Scroll View").GetComponent<SwipeUI>();
            // Set starting slot if null
            if (initialSlot == null)
            {
                initialSlot = transform.parent;
            }
            CheckSelectedCharacterSlotText();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            previousParent = transform.parent.parent.parent;
            transform.SetParent(transform.parent.parent.parent.parent);
            transform.SetAsLastSibling();
            _backgroundSpriteImage.raycastTarget = false;

            // Set the button colors to make the background transparent during dragging
            ColorBlock transparentColors = originalColors;
            transparentColors.disabledColor = new Color(0, 0, 0, 0);
            button.colors = transparentColors;
            //previousParent = transform.parent;
            _swipe.DragWithBlock(eventData, _blockType);
            GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(null);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }
        public void CheckSelectedCharacterSlotText()
        {
            var text1 = GameObject.FindGameObjectWithTag("TextSuoja1");
            var text2 = GameObject.FindGameObjectWithTag("TextSuoja2");
            var text3 = GameObject.FindGameObjectWithTag("TextSuoja3");

            /*characterTextCounter = (isAdditive) ? characterTextCounter + 1 : characterTextCounter - 1;
            
            if (characterTextCounter < 0)
                characterTextCounter = 0;

            if (characterTextCounter > 3)
                characterTextCounter = 3;*/

            if (_modelView._CurSelectedCharacterSlot[2].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(false);
            }
            else if (_modelView._CurSelectedCharacterSlot[1].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(true);
            }
            else if (_modelView._CurSelectedCharacterSlot[0].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(true);
                text3.SetActive(true);
            }
            else
            {
                text1.SetActive(true);
                text2.SetActive(true);
                text3.SetActive(true);
            }

            /*if (characterTextCounter > 2)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(false);
            }
            else if (characterTextCounter > 1)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(true);
            }
            else if (characterTextCounter > 0)
            {
                text1.SetActive(false);
                text2.SetActive(true);
                text3.SetActive(true);
            }
            else
            {
                text1.SetActive(true);
                text2.SetActive(true);
                text3.SetActive(true);
            }*/
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Transform droppedSlot = null;

            if (eventData.pointerEnter != null)
            {
                DraggableCharacter targetCharacter = eventData.pointerEnter.GetComponent<DraggableCharacter>();
                if (targetCharacter != null)
                {
                    // Check if targetCharacter's parent is tagged as "Topslot"
                    if (targetCharacter.transform.parent.CompareTag("Topslot"))
                    {
                        droppedSlot = targetCharacter.transform.parent;
                        droppedSlot.GetComponent<CharacterSlot>()?.SetCharacterDown();
                    }
                }
                else
                {
                    CharacterSlot characterSlot = eventData.pointerEnter.GetComponent<CharacterSlot>();
                    if (characterSlot != null)
                    {
                        droppedSlot = characterSlot.transform;
                    }
                }
            }

            if (droppedSlot == null || (droppedSlot != allowedSlot && droppedSlot.tag != "Topslot"))
            {
                transform.SetParent(initialSlot);
                transform.position = initialSlot.position;
            }
            else
            {
                // If droppedSlot is Topslot, find empty Topslot
                if (droppedSlot.tag == "Topslot")
                {
                    DraggableCharacter topSlotCharacter = droppedSlot.GetComponentInChildren<DraggableCharacter>();
                    if (topSlotCharacter != null)
                    {
                        // Move topSlotCharacter to it's initialSlot
                        Transform topSlotInitialSlot = topSlotCharacter.initialSlot;
                        topSlotCharacter.transform.SetParent(topSlotInitialSlot);
                        topSlotCharacter.transform.position = topSlotInitialSlot.position;
                    }

                    // Find the first empty topslot
                    Transform targetSlot = null;
                    foreach (var slot in _modelView._CurSelectedCharacterSlot)
                    {
                        if (slot.transform.childCount == 0)
                        {
                            targetSlot = slot.transform;
                            break;
                        }
                    }

                    // If an empty topslot is found, use it as the parent
                    if (targetSlot != null)
                    {
                        droppedSlot = targetSlot;
                    }
                }
                transform.SetParent(droppedSlot);
                transform.position = droppedSlot.position;

            }

            _backgroundSpriteImage.raycastTarget = true;
            button.colors = originalColors;

            if (transform.parent != previousParent)
            {
                previousParent = transform.parent;
                HandleParentChange(previousParent);
            }
            CheckSelectedCharacterSlotText();
        }

        private void HandleParentChange(Transform newParent)
        {
            // Go through each topslot
            foreach (var slot in _modelView._CurSelectedCharacterSlot)
            {
                // Check if newParent is one of the topslots
                if (newParent == slot.transform)
                {
                    OnParentChanged?.Invoke(newParent);
                }
            }

        }


        public void SetInfo(Sprite sprite, string name, CharacterID id, ModelView view)
        {
            _spriteImage.sprite = sprite;
            _characterNameText.text = name;
            _id = id;
            _modelView = view;
        }
    }
}
