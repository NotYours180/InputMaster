﻿#if UNITY_4_6
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.EventSystems;

namespace BSGTools.IO {
	[AddComponentMenu("InputMaster/UI/Input Master Standalone Input Module")]
	public class InputMasterInputModule : PointerInputModule {
		private float m_NextAction;

		private InputMode m_CurrentInputMode = InputMode.Buttons;

		private Vector2 m_LastMousePosition;
		private Vector2 m_MousePosition;

		protected InputMasterInputModule() { }

		public enum InputMode {
			Mouse,
			Buttons
		}

		public InputMode inputMode {
			get { return m_CurrentInputMode; }
		}

		private CombinedOutput m_HorizontalAxis;

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		private CombinedOutput m_VerticalAxis;

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		private CombinedOutput m_SubmitButton;

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		private CombinedOutput m_CancelButton;

		[SerializeField]
		private float m_InputActionsPerSecond = 10;

		[SerializeField]
		private bool m_AllowActivationOnMobileDevice;

		public bool allowActivationOnMobileDevice {
			get { return m_AllowActivationOnMobileDevice; }
			set { m_AllowActivationOnMobileDevice = value; }
		}

		public float inputActionsPerSecond {
			get { return m_InputActionsPerSecond; }
			set { m_InputActionsPerSecond = value; }
		}

		/// <summary>
		/// Name of the horizontal axis for movement (if axis events are used).
		/// </summary>
		public CombinedOutput horizontalAxis {
			get { return m_HorizontalAxis; }
			set { m_HorizontalAxis = value; }
		}

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		public CombinedOutput verticalAxis {
			get { return m_VerticalAxis; }
			set { m_VerticalAxis = value; }
		}

		public CombinedOutput submitButton {
			get { return m_SubmitButton; }
			set { m_SubmitButton = value; }
		}

		public CombinedOutput cancelButton {
			get { return m_CancelButton; }
			set { m_CancelButton = value; }
		}

		public override void UpdateModule() {
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = Input.mousePosition;
		}

		public override bool IsModuleSupported() {
			return m_AllowActivationOnMobileDevice || !Application.isMobilePlatform;
		}

		public override bool ShouldActivateModule() {
			if(!base.ShouldActivateModule())
				return false;

			var shouldActivate = m_SubmitButton.AnyDownPositive;
			shouldActivate |= m_CancelButton.AnyDownPositive;
			shouldActivate |= !Mathf.Approximately(m_HorizontalAxis.FixedValue, 0.0f);
			shouldActivate |= !Mathf.Approximately(m_VerticalAxis.FixedValue, 0.0f);
			shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
			shouldActivate |= Input.GetMouseButtonDown(0);
			return shouldActivate;
		}

		public override void ActivateModule() {
			base.ActivateModule();
			m_MousePosition = Input.mousePosition;
			m_LastMousePosition = Input.mousePosition;

			var toSelect = eventSystem.currentSelectedGameObject;
			if(toSelect == null)
				toSelect = eventSystem.lastSelectedGameObject;
			if(toSelect == null)
				toSelect = eventSystem.firstSelectedGameObject;

			eventSystem.SetSelectedGameObject(null, GetBaseEventData());
			eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
		}

		public override void DeactivateModule() {
			base.DeactivateModule();
			ClearSelection();
		}

		public override void Process() {
			bool usedEvent = SendUpdateEventToSelectedObject();

			if(eventSystem.sendNavigationEvents) {
				if(!usedEvent)
					usedEvent |= SendMoveEventToSelectedObject();

				if(!usedEvent)
					SendSubmitEventToSelectedObject();
			}

			ProcessMouseEvent();
		}

		/// <summary>
		/// Process submit keys.
		/// </summary>
		private bool SendSubmitEventToSelectedObject() {
			if(eventSystem.currentSelectedGameObject == null || m_CurrentInputMode != InputMode.Buttons)
				return false;

			var data = GetBaseEventData();
			if(m_SubmitButton.AnyDownPositive)
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

			if(m_CancelButton.AnyDownPositive)
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
			return data.used;
		}

		private bool AllowMoveEventProcessing(float time) {
			bool allow = m_HorizontalAxis.AnyDownPositive;
			allow |= m_VerticalAxis.AnyDownPositive;
			allow |= (time > m_NextAction);
			return allow;
		}

		private Vector2 GetRawMoveVector() {
			Vector2 move = Vector2.zero;
			move.x = m_HorizontalAxis.FixedValue;
			move.y = m_VerticalAxis.FixedValue;

			if(m_HorizontalAxis.AnyDownPositive) {
				if(move.x < 0)
					move.x = -1f;
				if(move.x > 0)
					move.x = 1f;
			}
			if(m_VerticalAxis.AnyDownPositive) {
				if(move.y < 0)
					move.y = -1f;
				if(move.y > 0)
					move.y = 1f;
			}
			return move;
		}

		/// <summary>
		/// Process keyboard events.
		/// </summary>
		private bool SendMoveEventToSelectedObject() {
			float time = Time.unscaledTime;

			if(!AllowMoveEventProcessing(time))
				return false;

			Vector2 movement = GetRawMoveVector();
			//Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
			var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
			if(!Mathf.Approximately(axisEventData.moveVector.x, 0f)
				|| !Mathf.Approximately(axisEventData.moveVector.y, 0f)) {
				if(m_CurrentInputMode != InputMode.Buttons) {
					// so if we are chaning to keyboard
					m_CurrentInputMode = InputMode.Buttons;

					// if we are doing a 'fresh selection'
					// return as we don't want to do a move.
					if(ResetSelection()) {
						m_NextAction = time + 1f / m_InputActionsPerSecond;
						return true;
					}
				}
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			}
			m_NextAction = time + 1f / m_InputActionsPerSecond;
			return axisEventData.used;
		}

		private bool ResetSelection() {
			var baseEventData = GetBaseEventData();
			// clear all selection
			// & figure out what the mouse is over
			var lastMousePointer = GetLastPointerEventData(kMouseLeftId);
			var hoveredObject = lastMousePointer == null ? null : lastMousePointer.pointerEnter;
			HandlePointerExitAndEnter(lastMousePointer, null);
			eventSystem.SetSelectedGameObject(null, baseEventData);

			// if we were hovering something... 
			// use this as the basis for the selection
			bool resetSelection = false;
			GameObject toSelect = ExecuteEvents.GetEventHandler<ISelectHandler>(hoveredObject);
			if(toSelect == null) {
				// if there was no hover
				// then use the last selected
				toSelect = eventSystem.lastSelectedGameObject;
				resetSelection = true;
			}

			eventSystem.SetSelectedGameObject(toSelect, baseEventData);
			return resetSelection;
		}

		/// <summary>
		/// Process all mouse events.
		/// </summary>
		private void ProcessMouseEvent() {
			var mouseData = GetMousePointerEventData();

			var pressed = mouseData.AnyPressesThisFrame();
			var released = mouseData.AnyReleasesThisFrame();

			var leftButtonData = mouseData[PointerEventData.InputButton.Left];

			if(!UseMouse(pressed, released, leftButtonData.buttonData))
				return;

			// Process the first mouse button fully
			ProcessMousePress(leftButtonData);
			ProcessMove(leftButtonData.buttonData);
			ProcessDrag(leftButtonData.buttonData);

			// Now process right / middle clicks
			ProcessMousePress(mouseData[PointerEventData.InputButton.Right]);
			ProcessDrag(mouseData[PointerEventData.InputButton.Right].buttonData);
			ProcessMousePress(mouseData[PointerEventData.InputButton.Middle]);
			ProcessDrag(mouseData[PointerEventData.InputButton.Middle].buttonData);

			if(!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f)) {
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		private bool UseMouse(bool pressed, bool released, PointerEventData pointerData) {
			if(m_CurrentInputMode == InputMode.Mouse)
				return true;

			// On mouse action switch back to mouse control scheme
			if(pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling()) {
				m_CurrentInputMode = InputMode.Mouse;
				eventSystem.SetSelectedGameObject(null);
			}

			return m_CurrentInputMode == InputMode.Mouse;
		}

		private bool SendUpdateEventToSelectedObject() {
			if(eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
			return data.used;
		}

		/// <summary>
		/// Process the current mouse press.
		/// </summary>
		private void ProcessMousePress(MouseButtonEventData data) {
			var pointerEvent = data.buttonData;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

			// PointerDown notification
			if(data.PressedThisFrame()) {
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				// search for the control that will receive the press
				// if we can't find a press handler set the press 
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

				// didnt find a press handler... search for a click handler
				if(newPressed == null)
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				//Debug.Log("Pressed: " + newPressed);

				float time = Time.unscaledTime;

				if(newPressed == pointerEvent.lastPress) {
					var diffTime = time - pointerEvent.clickTime;
					if(diffTime < 0.3f)
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else {
					pointerEvent.clickCount = 1;
				}

				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;

				pointerEvent.clickTime = time;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

				if(pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);

				//	Debug.Log("Setting Drag Handler to: " + pointer.pointerDrag);

				// Selection tracking
				var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
				eventSystem.SetSelectedGameObject(selectHandlerGO, pointerEvent);
			}

			// PointerUp notification
			if(data.ReleasedThisFrame()) {
				//Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

				//Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// PointerClick and Drop events
				if(pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if(pointerEvent.pointerDrag != null) {
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}

				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				pointerEvent.dragging = false;

				if(pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

				pointerEvent.pointerDrag = null;

				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if(currentOverGo != pointerEvent.pointerEnter) {
					HandlePointerExitAndEnter(pointerEvent, null);
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
				}
			}
		}
	}
}
#endif