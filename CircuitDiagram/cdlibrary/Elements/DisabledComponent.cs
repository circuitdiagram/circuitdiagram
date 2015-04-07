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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CircuitDiagram;
using System.ComponentModel;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a component which cannot be shown or modified.This enables the component to be written when the document is saved.
    /// </summary>
    public class DisabledComponent
    {
        static object updatedEventKey = new object();

        private EventHandlerList eventDelegates = new EventHandlerList();

        /// <summary>
        /// The collection this component belongs to.
        /// </summary>
        public string ImplementationCollection { get; set; }

        /// <summary>
        /// The item within the collection this component belongs to.
        /// </summary>
        public string ImplementationItem { get; set; }

        /// <summary>
        /// The name of the component description from which this component was created.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The GUID of the component description from which this component was created.
        /// </summary>
        public Guid? GUID { get; set; }

        /// <summary>
        /// The location of the component within the document.
        /// </summary>
        public Vector? Location { get; set; }

        /// <summary>
        /// The size of the component.
        /// </summary>
        public double? Size { get; set; }
        /// <summary>
        /// 
        /// Whether the component is flipped or not.
        /// </summary>
        public bool? IsFlipped { get; set; }

        /// <summary>
        /// Whether the component horizontal.
        /// </summary>
        public Orientation? Orientation { get; set; }

        /// <summary>
        /// Component properties.
        /// </summary>
        public IDictionary<string, object> Properties { get; private set; }

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

        /// <summary>
        /// Creates a new DisabledComponent.
        /// </summary>
        public DisabledComponent()
        {
            Properties = new Dictionary<string, object>();
        }
    }
}

