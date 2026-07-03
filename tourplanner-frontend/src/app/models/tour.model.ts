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
  routeGeometry: string | null;
  createdAt: string;
  // Berechnete Attribute (vom Backend geliefert)
  popularityCount: number;
  popularityLevel: string;
  childFriendliness: string;
}
