export interface Tour {
  id: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: string;
  distance: number;
  estimatedTime: number;
  routeImage: string | null;
  createdAt: string;
}
