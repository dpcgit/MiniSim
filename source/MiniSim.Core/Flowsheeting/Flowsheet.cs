﻿using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Numerics;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting.Documentation;

namespace MiniSim.Core.Flowsheeting
{
    public class Flowsheet : BaseSimulationElement
    {
        List<ProcessUnit> _units = new List<ProcessUnit>();
        List<MaterialStream> _materialStreams = new List<MaterialStream>();
        List<HeatStream> _heatStreams = new List<HeatStream>();
        List<Equation> _designSpecifications = new List<Equation>();
        List<DocumentationElement> _documentation = new List<DocumentationElement>();
        List<Variable> _customVariables = new List<Variable>();

        public List<ProcessUnit> Units
        {
            get
            {
                return _units;
            }

            set
            {
                _units = value;
            }
        }

        public List<MaterialStream> MaterialStreams
        {
            get
            {
                return _materialStreams;
            }

            set
            {
                _materialStreams = value;
            }
        }

        public List<HeatStream> HeatStreams
        {
            get
            {
                return _heatStreams;
            }

            set
            {
                _heatStreams = value;
            }
        }

        public List<Equation> DesignSpecifications
        {
            get
            {
                return _designSpecifications;
            }

            set
            {
                _designSpecifications = value;
            }
        }

        public List<DocumentationElement> Documentation { get => _documentation; set => _documentation = value; }
        public List<Variable> CustomVariables { get => _customVariables; set => _customVariables = value; }

        public Flowsheet(string name)
        {
            Name = name;
        }

        public Flowsheet AddCustomVariable(Variable var)
        {
            if (!CustomVariables.Contains(var))
            {
                CustomVariables.Add(var);
            }
            else
                throw new InvalidOperationException($"Custom Variable {var.Name} already included in flowsheet");

            return this;
        }

        public Flowsheet RemoveCustomVariable(Variable var)
        {
            if (CustomVariables.Contains(var))
            {
                CustomVariables.Remove(var);
            }
            else
                throw new InvalidOperationException($"Custom Variable {var.Name} not included in flowsheet");

            return this;
        }


        public Flowsheet AddDesignSpecification(string name, Equation eq)
        {
            eq.Name = name;
            eq.Group = "Design Spec";

            AddDesignSpecification(eq);
            return this;
        }
        public Flowsheet AddDesignSpecification(string name, Equation eq, string description)
        {
            eq.Name = name;
            eq.Group = "Design Spec";
            eq.Description = description;
            AddDesignSpecification(eq);
            return this;
        }

        public Flowsheet AddDesignSpecification(Equation eq)
        {
            if (!DesignSpecifications.Contains(eq))
            {
                eq.ModelClass = "Flowsheet";
                DesignSpecifications.Add(eq);
            }
            else
                throw new InvalidOperationException("Design spec " + eq.Name + " already included in flowsheet");

            return this;
        }

        public Flowsheet ReplaceDesignSpecification(string name, Equation eq)
        {
            var spec = DesignSpecifications.FirstOrDefault(s => s.Name == name);
            if (spec != null)
            {
                DesignSpecifications.Remove(spec);
                eq.Name = name;
                eq.Group = "Design Spec";
                AddDesignSpecification(eq);
            }

            return this;
        }


        public Flowsheet RemoveDesignSpecification(string name)
        {
            var spec = DesignSpecifications.FirstOrDefault(eq => eq.Name == name);
            if (spec != null)
                DesignSpecifications.Remove(spec);

            return this;
        }
        public Flowsheet RemoveDesignSpecification(Equation eq)
        {
            if (DesignSpecifications.Contains(eq))
                DesignSpecifications.Remove(eq);
            return this;
        }


        public Flowsheet AddUnits(params ProcessUnit[] units)
        {
            foreach (var unit in units)
                AddUnit(unit);

            return this;
        }

        public Flowsheet AddDocumentation(DocumentationElement unit)
        {
            if (!Documentation.Contains(unit))
                Documentation.Add(unit);
            else
                throw new InvalidOperationException("Element " + unit.Name + " already included in flowsheet");
            return this;
        }
        public Flowsheet AddDocumentationElements(params DocumentationElement[] units)
        {
            foreach (var unit in units)
                AddDocumentation(unit);
            return this;
        }

        public Flowsheet AddUnit(ProcessUnit unit)
        {
            if (!Units.Contains(unit))
                Units.Add(unit);
            else
                throw new InvalidOperationException("Unit " + unit.Name + " already included in flowsheet");
            return this;
        }

        public Flowsheet AddMaterialStreams(params MaterialStream[] streams)
        {
            foreach (var stream in streams)
                AddMaterialStream(stream);
            return this;
        }

        public Flowsheet RemoveMaterialStream(MaterialStream stream)
        {
            if (MaterialStreams.Contains(stream))
                MaterialStreams.Remove(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " not included in flowsheet");
            return this;

        }
        public Flowsheet AddMaterialStream(MaterialStream stream)
        {
            if (!MaterialStreams.Contains(stream))
                MaterialStreams.Add(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " already included in flowsheet");
            return this;
        }
        public Flowsheet AddHeatStreams(params HeatStream[] streams)
        {
            foreach (var stream in streams)
                AddHeatStream(stream);
            return this;
        }
        public Flowsheet AddHeatStream(HeatStream stream)
        {
            if (!HeatStreams.Contains(stream))
                HeatStreams.Add(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " already included in flowsheet");
            return this;
        }

        public Flowsheet Merge(Flowsheet other)
        {
            foreach (var unit in other.Units)
                AddUnit(unit);
            foreach (var stream in other.MaterialStreams)
                AddMaterialStream(stream);
            foreach (var stream in other.HeatStreams)
                AddHeatStream(stream);
            foreach (var spec in other.DesignSpecifications)
                AddDesignSpecification(spec);
            return this;
        }

        public List<ProcessUnit> GetSuccessorUnits(ProcessUnit  unit)
        {
            return MaterialStreams.Where(s => s.Source == unit && s.Sink != null).Select(s => s.Sink).ToList(); 
        }

        public List<ProcessUnit> GetSuccessorUnits(string name)
        {
            var successors = new List<ProcessUnit>();
            var unit = GetUnit(name);

            if (unit == null)
                throw new ArgumentOutOfRangeException($"Unit {name} not found in flowsheet");
                       
            return GetSuccessorUnits(unit);
        }
        public List<ProcessUnit> GetUnitsByModelClass(string modelClass)
        {
            return Units.Where(u => u.Class == modelClass).ToList();
        }

        public ProcessUnit GetUnit(string name)
        {
            return Units.FirstOrDefault(v => v.Name == name);
        }
        public MaterialStream GetMaterialStream(string name)
        {
            return MaterialStreams.FirstOrDefault(v => v.Name == name);
        }
        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var stream in MaterialStreams)
                stream.CreateEquations(problem);

            foreach (var stream in HeatStreams)
                stream.CreateEquations(problem);

            foreach (var unit in Units)
                unit.CreateEquations(problem);

            foreach (var spec in DesignSpecifications)
                AddEquationToEquationSystem(problem, spec);

            foreach (var vari in CustomVariables)
                problem.Variables.Add(vari);

            base.CreateEquations(problem);
        }

        public Flowsheet Initialize()
        {
            foreach (var unit in Units)
            {
                unit.Initialize();
            }
            return this;
        }

        public Dictionary<string, double> TakeSnapshot()
        {
            var snapshot = new Dictionary<string, double>();

            var eq = new AlgebraicSystem("Snapshot");
            CreateEquations(eq);

            foreach (var vari in eq.Variables)
                snapshot.Add(vari.ModelName + "_" + vari.FullName, vari.Val());
            return snapshot;
        }

        public Flowsheet RestoreSnapshot(Dictionary<string, double> snapshot)
        {
            var eq = new AlgebraicSystem("Snapshot");
            CreateEquations(eq);
            foreach (var vari in eq.Variables)
            {
                if (snapshot.ContainsKey(vari.ModelName + "_" + vari.FullName))
                {
                    vari.SetValue(snapshot[vari.ModelName + "_" + vari.FullName]);
                }
            }
            return this;
        }
    }
}
