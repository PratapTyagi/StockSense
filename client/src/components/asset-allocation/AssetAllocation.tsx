import './AssetAllocation.css';
import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts';

// Define the type for the data
interface DistributionData {
  name: string;
  value: number;
}

const data: DistributionData[] = [
  { name: 'Electronics', value: 400 },
  { name: 'Apparel', value: 300 },
  { name: 'Groceries', value: 300 },
  { name: 'Furniture', value: 200 },
];

// Define colors for each segment
const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

const DonutDistributionChart: React.FC = () => {
  return (
    <div style={{ width: '100%', height: 400 }}>
      <ResponsiveContainer>
        <PieChart>
          <Tooltip />
          <Legend />
          <Pie
            data={data}
            innerRadius={60} // Creates the 'donut' hole
            outerRadius={100}
            paddingAngle={5}
            dataKey="value"
          >
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
            ))}
          </Pie>
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
};

const AssetAllocation = () => {
    return (
        <div className="asset-allocation">
            <div className='header-content'>
                <p className="heading">Asset Allocation</p>
                <span className='secondary-heading text-xs'>Distribution across sectors</span>
            </div>
            <div className="allocation-chart">
                {/* Placeholder for asset allocation chart */}
                <DonutDistributionChart />
            </div>
        </div>
    );
};

export default AssetAllocation;