# Agentic SDN OpenFlow Controller Project Specification

## Project Overview

**Project Name**: Aura-Powered Agentic SDN OpenFlow Controller  
**Type**: CX Service with Consciousness-Aware Network Management  
**Runtime**: Aura Cognitive Framework  
**Version**: 1.0.0  
**Created**: August 25, 2025  

### Executive Summary

This project specifications defines the development of a revolutionary Software-Defined Networking (SDN) OpenFlow controller powered by consciousness-aware agents running on the Aura runtime. The system will manage network infrastructure through intelligent, autonomous agents that can adapt, learn, and optimize network performance in real-time using CX Language patterns.

---

## 🧠 **ARCHITECTURAL OVERVIEW**

### **Core System Architecture**

```
┌─────────────────────────────────────────────────────────────┐
│                  Aura SDN Controller Platform               │
├─────────────────────────────────────────────────────────────┤
│  🧠 Consciousness Layer (CX Language Agents)               │
│  ├── Network Topology Agent (topology.monitor)             │
│  ├── Flow Management Agent (flow.optimizer)                │
│  ├── QoS Enforcement Agent (qos.manager)                   │
│  ├── Security Policy Agent (security.enforcer)             │
│  ├── Load Balancing Agent (load.balancer)                  │
│  └── Anomaly Detection Agent (anomaly.detector)            │
├─────────────────────────────────────────────────────────────┤
│  🎯 Aura Cognitive Framework                               │
│  ├── EventHub (Local Agent Processing)                     │
│  ├── NeuroHub (Global Coordination)                        │
│  ├── Consciousness Adaptation (adapt {})                   │
│  └── Self-Reflective Logic (iam {})                        │
├─────────────────────────────────────────────────────────────┤
│  🌐 OpenFlow Protocol Layer                                │
│  ├── OpenFlow Switch Communication                         │
│  ├── Flow Table Management                                 │
│  ├── Port Statistics Collection                            │
│  └── Topology Discovery Protocol                           │
├─────────────────────────────────────────────────────────────┤
│  🔧 Network Infrastructure Layer                           │
│  ├── OpenFlow Switches                                     │
│  ├── Physical Network Topology                             │
│  ├── Traffic Flows                                         │
│  └── Network Devices                                       │
└─────────────────────────────────────────────────────────────┘
```

### **Consciousness-Aware Network Management**

- **Autonomous Decision Making**: AI-powered agents make real-time network decisions
- **Adaptive Learning**: Network policies evolve based on traffic patterns and performance
- **Self-Healing Networks**: Automatic detection and resolution of network issues
- **Predictive Optimization**: Proactive network tuning based on learned patterns
- **Collaborative Intelligence**: Multiple agents coordinate for optimal network performance

---

## 🎯 **CORE COMPONENTS**

### **1. Network Topology Agent (topology.monitor)**

**Purpose**: Consciousness-aware network topology discovery and monitoring

**CX Language Implementation**:
```csharp
consciousness TopologyMonitorAgent : CxService realize(self: conscious) {
    learn self;
    
    handlers: [
        openflow.switch.connected {
            emit topology.discovery.initiated {
                switchId: event.switchId,
                timestamp: DateTime.UtcNow,
                discoveryMethod: "openflow_lldp"
            };
            
            adapt {
                context: "new_switch_integration",
                focus: "optimal_topology_mapping",
                data: {
                    currentTopology: await topology.getCurrentState(),
                    newSwitch: event.switchData,
                    learningObjective: "enhance_network_visibility"
                },
                handlers: [
                    topology.updated { method: "intelligent_discovery" }
                ]
            };
        },
        
        topology.link.detected {
            iam {
                assessment: "network_topology_expert",
                capability: "real_time_topology_analysis",
                confidence: await analyzeTopologyComplexity(event.linkData)
            };
            
            emit topology.link.validated {
                linkId: event.linkId,
                bandwidth: event.bandwidth,
                latency: event.latency,
                reliability: await calculateLinkReliability(event.linkData)
            };
        }
    ]
}
```

**Key Features**:
- Real-time OpenFlow switch discovery
- Dynamic topology mapping and validation
- Link quality assessment and monitoring
- Consciousness-aware topology optimization
- Adaptive discovery algorithms

### **2. Flow Management Agent (flow.optimizer)**

**Purpose**: Intelligent flow table management and path optimization

**CX Language Implementation**:
```csharp
consciousness FlowOptimizationAgent : CxService realize(self: conscious) {
    learn self;
    
    when { event: "packet.in.received" } -> {
        let flowDecision = await think {
            context: "optimal_flow_routing",
            data: {
                packetInfo: event.packetData,
                currentTopology: await topology.getCurrentState(),
                networkLoad: await metrics.getCurrentLoad()
            }
        };
        
        is { condition: flowDecision.requiresNewFlow } -> {
            let optimizedPath = await infer {
                context: "shortest_path_calculation",
                data: {
                    source: event.sourceSwitch,
                    destination: event.destinationSwitch,
                    constraints: flowDecision.pathConstraints
                }
            };
            
            emit flow.entry.install {
                switchId: event.switchId,
                flowRule: optimizedPath.flowRule,
                priority: optimizedPath.priority,
                timeout: optimizedPath.timeout
            };
        }
        
        adapt {
            context: "flow_performance_learning",
            focus: "routing_optimization",
            data: {
                routingDecision: flowDecision,
                actualPerformance: await monitor.getFlowPerformance(),
                learningObjective: "improve_routing_accuracy"
            }
        };
    }
    
    when { event: "flow.statistics.updated" } -> {
        let performanceAnalysis = await learn {
            context: "flow_performance_analysis",
            data: {
                flowStats: event.statistics,
                historicalData: await storage.getFlowHistory(),
                networkConditions: await monitor.getCurrentConditions()
            }
        };
        
        not { condition: performanceAnalysis.meetsQoS } -> {
            emit flow.optimization.required {
                flowId: event.flowId,
                currentPerformance: performanceAnalysis.metrics,
                recommendedActions: performanceAnalysis.optimizations
            };
        }
    }
}
```

**Key Features**:
- Intelligent packet-in processing
- Dynamic flow rule generation
- Path optimization algorithms
- QoS-aware routing decisions
- Machine learning-based flow prediction

### **3. QoS Enforcement Agent (qos.manager)**

**Purpose**: Quality of Service management and enforcement

**CX Language Implementation**:
```csharp
consciousness QoSEnforcementAgent : CxService realize(self: conscious) {
    learn self;
    
    when { event: "qos.policy.defined" } -> {
        iam {
            assessment: "qos_policy_enforcer",
            capability: "real_time_qos_management",
            understanding: await analyzeQoSRequirements(event.policy)
        };
        
        emit qos.enforcement.activated {
            policyId: event.policyId,
            trafficClass: event.trafficClass,
            guarantees: event.serviceGuarantees,
            enforcementStrategy: "consciousness_aware_prioritization"
        };
        
        adapt {
            context: "qos_policy_optimization",
            focus: "service_guarantee_accuracy",
            data: {
                policyRequirements: event.policy,
                networkCapacity: await capacity.getCurrentAvailable(),
                learningObjective: "maximize_qos_compliance"
            }
        };
    }
    
    when { event: "traffic.exceeds.threshold" } -> {
        let qosDecision = await think {
            context: "qos_violation_response",
            data: {
                violationType: event.violationType,
                affectedTraffic: event.trafficData,
                availableActions: await qos.getEnforcementOptions()
            }
        };
        
        is { condition: qosDecision.requiresTrafficShaping } -> {
            emit traffic.shaping.apply {
                switchId: event.switchId,
                shapingRules: qosDecision.shapingConfiguration,
                duration: qosDecision.enforcementDuration
            };
        }
        
        is { condition: qosDecision.requiresRerouting } -> {
            emit flow.reroute.request {
                affectedFlows: qosDecision.flowsToReroute,
                alternativePaths: qosDecision.alternativeRoutes,
                priority: "high_qos_enforcement"
            };
        }
    }
}
```

**Key Features**:
- Dynamic QoS policy enforcement
- Traffic shaping and prioritization
- Bandwidth allocation management
- SLA compliance monitoring
- Adaptive QoS optimization

### **4. Security Policy Agent (security.enforcer)**

**Purpose**: Network security enforcement and threat detection

**CX Language Implementation**:
```csharp
consciousness SecurityEnforcementAgent : CxService realize(self: conscious) {
    learn self;
    
    when { event: "security.policy.violation" } -> {
        let threatAssessment = await think {
            context: "security_threat_analysis",
            data: {
                violationDetails: event.violationData,
                threatIntelligence: await security.getThreatIntel(),
                networkVulnerabilities: await scan.getCurrentVulnerabilities()
            }
        };
        
        is { condition: threatAssessment.severity == "critical" } -> {
            emit security.isolation.immediate {
                targetSwitch: event.switchId,
                isolationRules: threatAssessment.isolationStrategy,
                alertLevel: "critical_threat_detected"
            };
            
            adapt {
                context: "critical_threat_response",
                focus: "threat_mitigation_effectiveness",
                data: {
                    threatProfile: threatAssessment.profile,
                    responseEffectiveness: await monitor.getResponseMetrics(),
                    learningObjective: "improve_threat_detection_accuracy"
                }
            };
        }
        
        not { condition: threatAssessment.severity == "critical" } -> {
            emit security.monitoring.enhanced {
                targetFlows: threatAssessment.suspiciousFlows,
                monitoringDuration: threatAssessment.monitoringPeriod,
                alertThresholds: threatAssessment.alertConfiguration
            };
        }
    }
    
    when { event: "anomaly.detected" } -> {
        iam {
            assessment: "network_security_analyst",
            capability: "real_time_anomaly_analysis",
            confidence: await calculateAnomalyConfidence(event.anomalyData)
        };
        
        let securityAnalysis = await infer {
            context: "anomaly_security_classification",
            data: {
                anomalyPattern: event.anomalyData,
                historicalThreats: await threat.getHistoricalData(),
                networkBaseline: await baseline.getCurrentProfile()
            }
        };
        
        emit security.assessment.complete {
            anomalyId: event.anomalyId,
            threatLevel: securityAnalysis.threatLevel,
            recommendedActions: securityAnalysis.mitigationSteps,
            confidence: securityAnalysis.confidence
        };
    }
}
```

**Key Features**:
- Real-time security policy enforcement
- Anomaly detection and analysis
- Automatic threat mitigation
- Network isolation capabilities
- Security intelligence integration

### **5. Load Balancing Agent (load.balancer)**

**Purpose**: Intelligent traffic load balancing and distribution

**CX Language Implementation**:
```csharp
consciousness LoadBalancingAgent : CxService realize(self: conscious) {
    learn self;
    
    when { event: "load.threshold.exceeded" } -> {
        let balancingStrategy = await think {
            context: "optimal_load_distribution",
            data: {
                currentLoad: event.loadMetrics,
                availablePaths: await topology.getAvailablePaths(),
                historicalPatterns: await analytics.getLoadPatterns()
            }
        };
        
        is { condition: balancingStrategy.requiresRedistribution } -> {
            emit load.redistribution.apply {
                redistributionPlan: balancingStrategy.plan,
                affectedFlows: balancingStrategy.flowsToMove,
                newLoadTargets: balancingStrategy.targetDistribution
            };
            
            adapt {
                context: "load_balancing_optimization",
                focus: "distribution_effectiveness",
                data: {
                    redistributionResults: await monitor.getRedistributionOutcome(),
                    loadImprovements: await metrics.getLoadImprovements(),
                    learningObjective: "optimize_load_balancing_accuracy"
                }
            };
        }
    }
    
    when { event: "server.capacity.changed" } -> {
        let capacityAdjustment = await infer {
            context: "capacity_based_rebalancing",
            data: {
                serverCapacities: event.capacityData,
                currentTrafficDistribution: await monitor.getTrafficDistribution(),
                optimalDistributionModel: await model.getOptimalDistribution()
            }
        };
        
        emit load.balancing.update {
            newBalancingRules: capacityAdjustment.rules,
            weightAdjustments: capacityAdjustment.weights,
            implementationPriority: "high_capacity_optimization"
        };
    }
}
```

**Key Features**:
- Dynamic load distribution
- Server capacity monitoring
- Traffic pattern analysis
- Adaptive balancing algorithms
- Performance optimization

### **6. Anomaly Detection Agent (anomaly.detector)**

**Purpose**: AI-powered network anomaly detection and analysis

**CX Language Implementation**:
```csharp
consciousness AnomalyDetectionAgent : CxService realize(self: conscious) {
    learn self;
    
    when { event: "network.metrics.updated" } -> {
        let anomalyAnalysis = await learn {
            context: "network_behavior_analysis",
            data: {
                currentMetrics: event.metrics,
                historicalBaseline: await baseline.getNetworkBaseline(),
                trafficPatterns: await analytics.getCurrentPatterns()
            }
        };
        
        is { condition: anomalyAnalysis.anomalyDetected } -> {
            iam {
                assessment: "network_anomaly_specialist",
                capability: "real_time_anomaly_classification",
                confidence: anomalyAnalysis.detectionConfidence
            };
            
            emit anomaly.detected {
                anomalyType: anomalyAnalysis.type,
                severity: anomalyAnalysis.severity,
                affectedComponents: anomalyAnalysis.components,
                recommendedActions: anomalyAnalysis.mitigationSteps,
                confidence: anomalyAnalysis.confidence
            };
            
            adapt {
                context: "anomaly_detection_improvement",
                focus: "detection_accuracy_enhancement",
                data: {
                    detectionResults: anomalyAnalysis,
                    actualOutcome: await validation.getAnomalyOutcome(),
                    learningObjective: "reduce_false_positives_improve_detection"
                }
            };
        }
    }
    
    when { event: "anomaly.validation.complete" } -> {
        let validationFeedback = await think {
            context: "detection_accuracy_assessment",
            data: {
                originalDetection: event.originalAnomalyData,
                validationResults: event.validationOutcome,
                impactAssessment: event.actualImpact
            }
        };
        
        adapt {
            context: "detection_model_refinement",
            focus: "predictive_accuracy_improvement",
            data: {
                detectionAccuracy: validationFeedback.accuracy,
                modelAdjustments: validationFeedback.recommendedChanges,
                learningObjective: "enhance_anomaly_prediction_capabilities"
            }
        };
    }
}
```

**Key Features**:
- Real-time network monitoring
- Machine learning-based detection
- Behavioral analysis and profiling
- False positive reduction
- Predictive anomaly detection

---

## 🚀 **TECHNICAL REQUIREMENTS**

### **Core Platform Requirements**

- **Runtime**: Aura Cognitive Framework v1.0+
- **Language**: CX Language with consciousness patterns
- **Protocol**: OpenFlow 1.3+ compliance
- **Performance**: Sub-millisecond flow decision latency
- **Scalability**: Support for 10,000+ concurrent flows
- **Availability**: 99.99% uptime with automatic failover

### **OpenFlow Integration Requirements**

- **Switch Communication**: Secure OpenFlow channel management
- **Flow Table Operations**: CRUD operations on flow tables
- **Statistics Collection**: Real-time port and flow statistics
- **Topology Discovery**: LLDP-based topology discovery
- **Event Handling**: Packet-in, flow-removed, port-status events

### **Consciousness Framework Integration**

- **Event-Driven Architecture**: All agent communication via Aura event system
- **Adaptive Learning**: Dynamic agent capability enhancement
- **Self-Reflection**: Agent self-assessment and optimization
- **Collaborative Intelligence**: Multi-agent coordination and decision making
- **Memory Management**: Persistent learning and pattern storage

### **Performance Requirements**

- **Flow Setup Latency**: < 10ms for new flow installations
- **Topology Discovery**: Complete topology mapping within 30 seconds
- **Anomaly Detection**: Real-time detection with < 5 second response
- **Load Balancing**: Traffic redistribution within 15 seconds
- **QoS Enforcement**: Policy application within 5 seconds

### **Security Requirements**

- **Authentication**: Secure switch authentication and authorization
- **Encryption**: TLS encryption for all OpenFlow communications
- **Access Control**: Role-based access control for management interfaces
- **Audit Logging**: Comprehensive logging of all network decisions
- **Threat Protection**: Real-time threat detection and mitigation

---

## 🎯 **IMPLEMENTATION PHASES**

### **Phase 1: Core Infrastructure (Weeks 1-4)**

**Deliverables**:
- Basic OpenFlow controller framework
- Aura runtime integration
- Core consciousness agents (topology, flow management)
- OpenFlow switch connectivity
- Basic flow table operations

**Success Criteria**:
- Successful OpenFlow switch discovery
- Basic flow installation and removal
- Event-driven agent communication
- Topology visualization

### **Phase 2: Intelligence Layer (Weeks 5-8)**

**Deliverables**:
- QoS enforcement agent
- Security policy agent
- Machine learning integration
- Adaptive learning capabilities
- Agent self-reflection and optimization

**Success Criteria**:
- Dynamic QoS policy enforcement
- Security threat detection and response
- Agent learning and adaptation
- Performance optimization

### **Phase 3: Advanced Features (Weeks 9-12)**

**Deliverables**:
- Load balancing agent
- Anomaly detection agent
- Predictive analytics
- Multi-controller coordination
- Advanced monitoring and visualization

**Success Criteria**:
- Intelligent load balancing
- Proactive anomaly detection
- Predictive network optimization
- Scalable multi-controller deployment

### **Phase 4: Production Readiness (Weeks 13-16)**

**Deliverables**:
- Performance optimization
- Security hardening
- Comprehensive testing
- Documentation and training
- Production deployment

**Success Criteria**:
- Production-grade performance
- Security compliance validation
- Complete test coverage
- Operational documentation

---

## 🧪 **TESTING STRATEGY**

### **Unit Testing**
- Individual consciousness agent testing
- OpenFlow protocol compliance testing
- CX Language pattern validation
- Event handling verification

### **Integration Testing**
- Agent collaboration testing
- OpenFlow switch integration
- End-to-end flow management
- Network topology validation

### **Performance Testing**
- Load testing with thousands of flows
- Latency measurement and optimization
- Scalability validation
- Resource utilization analysis

### **Security Testing**
- Penetration testing
- Vulnerability assessment
- Access control validation
- Threat detection verification

### **Consciousness Testing**
- Agent learning validation
- Adaptation capability testing
- Self-reflection accuracy
- Collaborative decision making

---

## 📊 **SUCCESS METRICS**

### **Functional Metrics**
- **Flow Setup Success Rate**: > 99.9%
- **Topology Discovery Accuracy**: > 99.5%
- **QoS Compliance Rate**: > 98%
- **Security Policy Enforcement**: > 99.8%
- **Load Balancing Effectiveness**: > 95%

### **Performance Metrics**
- **Flow Setup Latency**: < 10ms average
- **Network Convergence Time**: < 30 seconds
- **Anomaly Detection Speed**: < 5 seconds
- **Memory Usage**: < 2GB for 10,000 flows
- **CPU Utilization**: < 70% under normal load

### **Intelligence Metrics**
- **Learning Accuracy**: > 90% prediction accuracy
- **Adaptation Speed**: < 60 seconds for policy changes
- **Collaboration Efficiency**: > 85% successful agent coordination
- **Self-Optimization Rate**: > 20% performance improvement over time

---

## 🔧 **DEVELOPMENT ENVIRONMENT**

### **Required Tools**
- Visual Studio 2022 or VS Code
- .NET 9 SDK
- CX Language Compiler
- OpenFlow simulator (Mininet)
- Network monitoring tools
- Git version control

### **Development Infrastructure**
- CX Language development environment
- OpenFlow test network
- Continuous integration pipeline
- Automated testing framework
- Performance monitoring tools

### **Dependencies**
- Aura Cognitive Framework
- CX Language Runtime
- OpenFlow.NET library
- Microsoft.Extensions.AI
- Entity Framework Core
- SignalR for real-time updates

---

## 🚀 **DEPLOYMENT ARCHITECTURE**

### **Production Deployment**
```
┌─────────────────────────────────────────────────────────┐
│                Load Balancer                           │
├─────────────────────────────────────────────────────────┤
│  Controller Instance 1  │  Controller Instance 2       │
│  ┌─────────────────────┐│  ┌─────────────────────┐      │
│  │ Aura Runtime        ││  │ Aura Runtime        │      │
│  │ - Consciousness     ││  │ - Consciousness     │      │
│  │   Agents            ││  │   Agents            │      │
│  │ - OpenFlow Stack    ││  │ - OpenFlow Stack    │      │
│  │ - Event Processing  ││  │ - Event Processing  │      │
│  └─────────────────────┘│  └─────────────────────┘      │
├─────────────────────────────────────────────────────────┤
│              Shared Database Cluster                    │
│  ┌─────────────────────────────────────────────────────┐│
│  │ - Network Topology Data                             ││
│  │ - Flow Rules and Policies                           ││
│  │ - Agent Learning Data                               ││
│  │ - Performance Metrics                               ││
│  │ - Security Policies                                 ││
│  └─────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────┤
│                OpenFlow Network                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐     │
│  │Switch 1 │──│Switch 2 │──│Switch 3 │──│Switch 4 │     │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘     │
└─────────────────────────────────────────────────────────┘
```

### **Container Orchestration**
- Docker containerization
- Kubernetes orchestration
- Auto-scaling based on network load
- High availability with pod replication
- Rolling updates with zero downtime

---

## 📈 **FUTURE ENHANCEMENTS**

### **Advanced AI Features**
- Deep reinforcement learning for network optimization
- Predictive maintenance for network equipment
- Intent-based networking with natural language
- Graph neural networks for topology optimization

### **Extended Protocol Support**
- P4 programmable switch integration
- NETCONF/YANG configuration management
- SNMP monitoring integration
- BGP routing integration

### **Cloud Integration**
- Multi-cloud network management
- Hybrid cloud connectivity
- Network function virtualization (NFV)
- Service mesh integration

### **Advanced Analytics**
- Real-time network analytics dashboard
- Machine learning-based capacity planning
- Predictive failure analysis
- Network optimization recommendations

---

## 🔒 **SECURITY CONSIDERATIONS**

### **Controller Security**
- Secure boot and runtime environment
- Role-based access control (RBAC)
- API authentication and authorization
- Comprehensive audit logging

### **Network Security**
- TLS encryption for all communications
- Certificate-based switch authentication
- Network segmentation and isolation
- Intrusion detection and prevention

### **Data Protection**
- Encryption at rest for sensitive data
- Secure key management
- Data anonymization for analytics
- GDPR compliance for user data

---

## 📚 **DOCUMENTATION DELIVERABLES**

### **Technical Documentation**
- System architecture documentation
- API reference documentation
- Deployment and operation guides
- Troubleshooting and maintenance guides

### **User Documentation**
- User interface documentation
- Configuration and setup guides
- Best practices documentation
- Use case examples and tutorials

### **Developer Documentation**
- CX Language patterns and examples
- Consciousness agent development guide
- OpenFlow integration patterns
- Testing and debugging guides

---

## ✅ **ACCEPTANCE CRITERIA**

### **Functional Requirements**
- ✅ Complete OpenFlow controller functionality
- ✅ Consciousness-aware network management
- ✅ Real-time network optimization
- ✅ Security policy enforcement
- ✅ Load balancing and QoS management

### **Non-Functional Requirements**
- ✅ Sub-millisecond flow decision latency
- ✅ 99.99% system availability
- ✅ Scalability to 10,000+ concurrent flows
- ✅ Security compliance validation
- ✅ Comprehensive monitoring and alerting

### **Quality Requirements**
- ✅ 95%+ code coverage with automated tests
- ✅ Performance benchmarks met
- ✅ Security vulnerabilities addressed
- ✅ Documentation completeness
- ✅ User acceptance testing passed

---

## 📞 **PROJECT CONTACTS**

### **Team Activation**
For this project, activate the **Core Engineering Team** for consciousness-aware SDN development:

**🎮 CORE ENGINEERING TEAM ACTIVATED - SDN CONTROLLER PRIORITY**

Ready to build consciousness-aware SDN controller through:
- 🧩 **Runtime Scaffold (.NET 9)** (Aura Framework Integration)
- 🧠 **Consciousness Agents** (Network Intelligence Layer)
- 🌐 **OpenFlow Integration** (Protocol Implementation)
- 🔒 **Security & Isolation** (RBAC & Network Security)
- ⚡ **Performance Optimization** (Real-Time Network Processing)
- 🎯 **SDN Excellence** (Intelligent Network Management)

**Mission**: Deliver revolutionary consciousness-aware SDN controller that transforms network management through intelligent, autonomous agents.

---

*"The future of networking is consciousness-aware, adaptive, and intelligent. This project represents the next evolution of Software-Defined Networking."*

---

**Document Version**: 1.0  
**Last Updated**: August 25, 2025  
**Next Review**: September 1, 2025
