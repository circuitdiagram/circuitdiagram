// UndoManager.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    public class UndoManager
    {
        internal delegate void UndoActionDelegate(object sender, UndoActionEventArgs e);
        UndoActionDelegate m_undoActionDelegate;

        List<UndoAction> m_actions;

        private int LastSavedIndex { get; set; }
        private int CurrentActionIndex { get; set; }

        public event EventHandler ActionOccurred;
        internal UndoActionDelegate ActionDelegate { get { return m_undoActionDelegate; } set { m_undoActionDelegate = value; } }

        public UndoManager()
        {
            m_actions = new List<UndoAction>();
            CurrentActionIndex = -1;
            LastSavedIndex = -1;
        }

        public void AddAction(UndoAction action)
        {
            if (m_actions.Count - 1 > CurrentActionIndex && CurrentActionIndex >= 0)
                m_actions.RemoveRange(CurrentActionIndex + 1, m_actions.Count - (CurrentActionIndex + 1));
            m_actions.Add(action);
            CurrentActionIndex = m_actions.Count - 1;
            if (ActionOccurred != null)
                ActionOccurred(this, new EventArgs());
        }

        public bool CanStepBackwards()
        {
            return (CurrentActionIndex >= 0 && m_actions.Count >= 1);
        }

        public void StepBackwards()
        {
            if (CanStepBackwards())
            {
                if (m_undoActionDelegate != null)
                {
                    m_undoActionDelegate(this, new UndoActionEventArgs(m_actions[CurrentActionIndex], UndoActionEvent.Remove));
                }
                CurrentActionIndex--;
                if (ActionOccurred != null)
                    ActionOccurred(this, new EventArgs());
            }
        }

        public bool CanStepForwards()
        {
            return (CurrentActionIndex >= -1 && CurrentActionIndex < m_actions.Count - 1);
        }

        public void StepForwards()
        {
            if (CanStepForwards())
            {
                CurrentActionIndex++;
                if (m_undoActionDelegate != null)
                {
                    m_undoActionDelegate(this, new UndoActionEventArgs(m_actions[CurrentActionIndex], UndoActionEvent.Apply));
                }
                if (ActionOccurred != null)
                    ActionOccurred(this, new EventArgs());
            }
        }

        public void SetSaveIndex()
        {
            LastSavedIndex = CurrentActionIndex;
        }

        public bool IsSavedState()
        {
            return (LastSavedIndex == CurrentActionIndex);
        }
    }

    public class UndoAction
    {
        int m_indexer = 0;

        public string Description { get; set; }
        private Dictionary<Type, string> dTypeKeys { get; set; }
        private Dictionary<string, object> Data { get; set; }
        public UndoCommand Command { get; set; }

        public UndoAction(UndoCommand command, string description, object data = null)
        {
            Command = command;
            Description = description;
            Data = new Dictionary<string, object>();
            dTypeKeys = new Dictionary<Type, string>();
            if (data != null)
            {
                Data.Add("/default", data);
                dTypeKeys.Add(data.GetType(), "/default");
            }
        }

        public void AddData(string key, object data)
        {
            if (!Data.ContainsKey(key))
                Data.Add(key, data);
        }

        public void AddData(object data)
        {
            if (dTypeKeys.ContainsKey(data.GetType()))
                return;
            dTypeKeys.Add(data.GetType(), "/indexer" + m_indexer.ToString());
            Data.Add("/indexer" + m_indexer.ToString(), data);
            m_indexer++;
        }

        public object GetDefaultData()
        {
            if (!Data.ContainsKey("/default"))
                return null;
            return Data["/default"];
        }

        public T GetData<T>(string key)
        {
            if (!Data.ContainsKey(key))
                return default(T);
            return (T)Data[key];
        }

        public T GetData<T>()
        {
            if (dTypeKeys.ContainsKey(typeof(T)))
            {
                return (T)Data[dTypeKeys[typeof(T)]];
            }
            else
                return default(T);
        }
    }

    public enum UndoCommand
    {
        AddComponents,
        DeleteComponents,
        ModifyComponents,
        ResizeDocument,
        ModifyMetadata
    }

    public enum UndoActionEvent
    {
        Apply,
        Remove
    }

    public class UndoActionEventArgs : EventArgs
    {
        public UndoAction Action { get; private set; }
        public UndoActionEvent Event { get; private set; }

        public UndoActionEventArgs(UndoAction action, UndoActionEvent actionEvent)
        {
            Action = action;
            Event = actionEvent;
        }
    }
}
