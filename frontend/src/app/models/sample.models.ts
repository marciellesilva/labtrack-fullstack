export type SampleStatus = 'Pending' | 'Approved' | 'Rejected';

export interface SampleSummary {
  id: number;
  code: string;
  productName: string;
  batchNumber: string;
  collectedAt: string;
  status: SampleStatus;
  resultsCount: number;
  outOfSpecCount: number;
}

export interface AnalysisResult {
  id: number;
  parameter: string;
  unit: string;
  value: number;
  minLimit: number | null;
  maxLimit: number | null;
  isWithinSpec: boolean;
  analyst: string;
  analyzedAt: string;
}

export interface SampleDetail {
  id: number;
  code: string;
  productName: string;
  batchNumber: string;
  collectedAt: string;
  notes: string | null;
  status: SampleStatus;
  results: AnalysisResult[];
}

export interface SampleRequest {
  code: string;
  productName: string;
  batchNumber: string;
  collectedAt: string;
  notes?: string | null;
}

export interface AddResultRequest {
  parameter: string;
  unit: string;
  value: number;
  minLimit?: number | null;
  maxLimit?: number | null;
  analyst: string;
}

export interface OutOfSpecItem {
  sampleId: number;
  sampleCode: string;
  productName: string;
  parameter: string;
  value: number;
  unit: string;
  minLimit: number | null;
  maxLimit: number | null;
  analyzedAt: string;
}

export interface Dashboard {
  totalSamples: number;
  pendingSamples: number;
  approvedSamples: number;
  rejectedSamples: number;
  totalResults: number;
  outOfSpecResults: number;
  approvalRate: number;
  recentOutOfSpec: OutOfSpecItem[];
}

export const STATUS_LABELS: Record<SampleStatus, string> = {
  Pending: 'Pendente',
  Approved: 'Aprovada',
  Rejected: 'Reprovada',
};
