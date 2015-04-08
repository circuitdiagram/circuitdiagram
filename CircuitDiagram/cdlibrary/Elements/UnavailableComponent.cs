#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion

using CircuitDiagram.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a component which cannot be modified.
    /// </summary>
    public class UnavailableComponent : IComponentElement
    {
        static object updatedEventKey = new object();

        private EventHandlerList eventDelegates = new EventHandlerList();

        public string ImplementationCollection
        {
            get { throw new NotImplementedException(); }
        }

        public string ImplementationItem
        {
            get { throw new NotImplementedException(); }
        }

        public double Size
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFlipped
        {
            get { throw new NotImplementedException(); }
        }

        public Orientation Orientation
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, PropertyUnion> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public System.Windows.Vector Location
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler Updated
        {
            add
            {
                eventDelegates.AddHandler(updatedEventKey, value);
            }
            remove
            {
                eventDelegates.RemoveHandler(updatedEventKey, value);
            }
        }

        public void Render(Render.IRenderContext dc, bool absolute)
        {
            throw new NotImplementedException();
        }
    }
}
