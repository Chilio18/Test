export type CallStatus = 'Uploaded' | 'Processing' | 'Transcribing' | 'Analyzing' | 'Completed' | 'Failed'
export type CallType = 'Discovery' | 'Demo' | 'Negotiation' | 'CheckIn' | 'Onboarding' | 'SupportCall' | 'QBR' | 'Other'
export type OpportunityStage = 'Prospecting' | 'Qualification' | 'NeedsAnalysis' | 'ValueProposition' | 'DecisionMakers' | 'PerceptionAnalysis' | 'ProposalQuote' | 'NegotiationReview' | 'ClosedWon' | 'ClosedLost'

export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; hasNextPage: boolean; hasPreviousPage: boolean }

export interface Call { id: string; title: string; status: CallStatus; type: CallType; meetingDate: string; durationSeconds: number | null; ownerName: string; accountName: string | null; opportunityName: string | null; dealHealthScore: number | null; actionItemCount: number; hasRecording: boolean }

export interface TranscriptSegment { speaker: string; startTime: number; endTime: number; text: string }

export interface CallInsight { type: string; content: string; quote: string | null; startTime: number | null; speaker: string | null; confidence: number }

export interface ActionItem { id: string; title: string; description: string | null; status: 'Open' | 'InProgress' | 'Completed' | 'Dismissed'; assigneeName: string | null; dueDate: string | null; isCrmSynced: boolean }

export interface ConversationSummary { executiveSummary: string; detailedSummary: string; meetingRecap: string; keyTopics: string; sentimentScore: number; sentimentLabel: string }

export interface DealRiskAnalysis { healthScore: number; riskScore: number; confidenceScore: number; risks: string[]; opportunities: string[]; hasNextStep: boolean; hasBudgetDiscussion: boolean; hasDecisionMaker: boolean; explanation: string }

export interface CoachingFeedback { discoveryScore: number; qualificationScore: number; objectionHandlingScore: number; closingScore: number; communicationScore: number; overallScore: number; strengths: string[]; improvementAreas: string[]; detailedFeedback: string }

export interface CrmSuggestions { suggestedNote: string; suggestedNextStep: string | null; suggestedStageChange: string | null; suggestedTasks: string[]; suggestedDealUpdate: string | null }

export interface ScoreItem { criterionName: string; aiScore: number; managerScore: number | null; reasoning: string | null }

export interface Scorecard { id: string; templateName: string; scorePercentage: number; isManagerReviewed: boolean; scores: ScoreItem[] }

export interface CallDetail {
  id: string; title: string; status: CallStatus; type: CallType; meetingDate: string; durationSeconds: number | null; language: string | null
  account: { id: string; name: string; website: string | null } | null
  opportunity: { id: string; name: string; stage: string; amount: number | null } | null
  owner: { id: string; fullName: string; email: string; avatarUrl: string | null }
  recordings: Array<{ id: string; isVideo: boolean; durationSeconds: number | null; presignedUrl: string }>
  transcript: { segments: TranscriptSegment[] } | null
  summary: ConversationSummary | null
  insights: CallInsight[]
  actionItems: ActionItem[]
  dealRisk: DealRiskAnalysis | null
  coaching: CoachingFeedback | null
  crmSuggestions: CrmSuggestions | null
  followUpEmail: string | null
  scorecard: Scorecard | null
  createdAt: string
}

export interface Account { id: string; name: string; website: string | null; industry: string | null; country: string | null; size: string | null; ownerId: string | null }

export interface Contact { id: string; firstName: string; lastName: string; fullName: string; email: string | null; phone: string | null; title: string | null; accountId: string | null; accountName: string | null; isDecisionMaker: boolean; isChampion: boolean }

export interface Opportunity { id: string; name: string; accountId: string; accountName: string; ownerId: string | null; ownerName: string | null; stage: OpportunityStage; amount: number | null; currency: string; closeDate: string | null; probability: number | null; dealHealthScore: number | null; riskScore: number | null; confidenceScore: number | null; isStalled: boolean; hasNextStep: boolean; hasBudgetDiscussion: boolean; hasDecisionMaker: boolean }

export interface CrmConnection { id: string; name: string; providerType: 'Zoho' | 'InternalDotNet' | 'Mock'; isActive: boolean; lastSyncAt: string | null; lastSyncError: string | null }

export interface ScorecardTemplate { id: string; name: string; description: string | null; isActive: boolean; isDefault: boolean; criteria: Array<{ id: string; name: string; description: string | null; maxScore: number; weight: number }> }

export interface LeadershipDashboard { totalCalls: number; totalOpportunities: number; totalPipelineValue: number; averageCallDurationMinutes: number; highRiskDeals: number; topPerformers: RepPerformance[]; dealsAtRisk: DealAtRisk[]; callTrend: CallTrend[] }

export interface RepPerformance { userId: string; name: string; callCount: number; averageScore: number; actionItemsCompleted: number }

export interface DealAtRisk { opportunityId: string; name: string; accountName: string; amount: number | null; riskScore: number; riskReasons: string[] }

export interface CallTrend { date: string; count: number; averageScore: number }

export interface SemanticSearchResponse { answer: string; sources: Array<{ documentId: string; content: string; score: number; sourceType: string; sourceId: string | null; metadata: Record<string, string> }>; query: string }

export interface CurrentUser { userId: string; tenantId: string; email: string; fullName: string; role: string; avatarUrl: string | null }

export interface Notification { id: string; type: string; title: string; body: string | null; actionUrl: string | null; isRead: boolean; readAt: string | null; createdAt: string }
