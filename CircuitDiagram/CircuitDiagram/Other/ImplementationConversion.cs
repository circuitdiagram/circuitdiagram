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
using System.Collections.ObjectModel;
using CircuitDiagram.TypeDescriptionIO;

namespace CircuitDiagram
{
    public class ImplementationConversionCollection
    {
        public string ImplementationSet { get; set; }
        public ObservableCollection<ImplementationConversion> Items { get; private set; }

        public ImplementationConversionCollection()
        {
            Items = new ObservableCollection<ImplementationConversion>();
        }

        public override string ToString()
        {
            return ImplementationSet;
        }
    }

    public class ImplementationConversion
    {
        public string ImplementationName { get; set; }

        public string ToName { get; set; }
        public Guid ToGUID { get; set; }
        public string ToConfiguration { get; set; }
        public MultiResolutionImage ToIcon { get; set; }

        public bool NoConfiguration { get { return String.IsNullOrEmpty(ToConfiguration); } }
        public bool HasConfiguration { get { return !NoConfiguration; } }
    }
}
