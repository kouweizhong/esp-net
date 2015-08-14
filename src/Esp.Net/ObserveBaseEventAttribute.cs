#region copyright
// Copyright 2015 Keith Woods
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;

namespace Esp.Net
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ObserveBaseEventAttribute : Attribute
    {
        public ObserveBaseEventAttribute(Type eventType, Type baseType) : this(eventType, baseType, ObservationStage.Normal)
        {
        }

        public ObserveBaseEventAttribute(Type eventType, Type baseType, ObservationStage stage)
        {
            EventType = eventType;
            BaseType = baseType;
            Stage = stage;
        }

        public Type EventType { get; private set; }

        public Type BaseType { get; private set; }

        public ObservationStage Stage { get; private set; }
    }
}