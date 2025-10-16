# ?? SIM Ternak Ayam - Dashboard System

## ?? Dashboard Overview by Role

### ?? **OPERATOR Dashboard (System Admin)**
**Purpose**: Complete system oversight and management

**Key Metrics Displayed**:
- **System Overview**: Total kandangs, ayams, users, active operations
- **Kandang Performances**: All kandang utilization, mortality rates, status
- **Financial Summary**: Revenue, expenses, profit, margins
- **System Alerts**: Low stock, high mortality, maintenance needs
- **Productivity Stats**: Average weights, FCR, overall efficiency

**Chart Visualizations**:
- Revenue vs Expenses (Bar Chart)
- System-wide Mortality Trends (Line Chart)
- Overall Production Performance (Area Chart)
- All Kandang Utilization (Doughnut Chart)

---

### ?? **PETUGAS Dashboard (Farm Manager)**
**Purpose**: Focus on assigned kandang(s) and daily operations

**Key Metrics Displayed**:
- **My Kandangs**: Personal kandang status, health, capacity
- **Daily Tasks**: Feeding, vaccination, cleaning schedules
- **Stock Alerts**: Personal pakan/vaksin inventory warnings
- **My Performance**: Personal efficiency scores, task completion
- **Upcoming Activities**: Today/tomorrow/week scheduling

**Chart Visualizations**:
- My Kandang Performance Comparison (Radar Chart)
- Daily Task Completion (Stacked Bar Chart)
- Feed Consumption by Kandang (Bar Chart)
- Personal Productivity Trends (Line Chart)

---

### ?? **PEMILIK Dashboard (Business Owner)**
**Purpose**: Strategic business insights and profitability analysis

**Key Metrics Displayed**:
- **Business KPIs**: ROI, market share, customer satisfaction
- **Profitability**: Gross/net profit, cost per kg, margins
- **Comparison Analysis**: Month-over-month, year-over-year growth
- **Strategic Insights**: Recommendations, opportunities, risks
- **Monthly Trends**: Revenue, profit, productivity patterns

**Chart Visualizations**:
- Financial Performance Multi-line (Line Chart)
- Seasonal Business Trends (Multi-area Chart)
- Cost Breakdown Analysis (Pie Chart)
- Performance vs Industry Benchmark (Comparison Chart)

---

## ?? **API Endpoints**

### Dashboard Data Endpoints
```
GET /api/dashboard                          # Auto-detect role from JWT
GET /api/dashboard/role/{role}              # Specific role dashboard
GET /api/dashboard/operator                 # Operator dashboard
GET /api/dashboard/petugas                  # Petugas dashboard  
GET /api/dashboard/pemilik                  # Pemilik dashboard
```

### Chart Data Endpoints
```
GET /api/dashboard/charts/revenue-expense           # Revenue vs Expenses
GET /api/dashboard/charts/mortality-trend           # Mortality Trends
GET /api/dashboard/charts/production                # Production Performance
GET /api/dashboard/charts/kandang-utilization       # Kandang Utilization
GET /api/dashboard/charts/feed-consumption          # Feed Consumption
GET /api/dashboard/charts/financial-performance     # Financial Performance
GET /api/dashboard/charts/operational-activities    # Operational Activities
GET /api/dashboard/charts/stock-levels              # Stock Levels
GET /api/dashboard/charts/performance-comparison    # Performance Comparison
GET /api/dashboard/charts/seasonal-trends           # Seasonal Trends
GET /api/dashboard/charts/cost-breakdown            # Cost Breakdown
```

---

## ?? **Chart Types & Data Structure**

### 1. Revenue vs Expenses Chart (Bar Chart)
```json
{
  "chartType": "bar",
  "title": "Revenue vs Expenses",
  "labels": ["Jan 2024", "Feb 2024", "Mar 2024"],
  "datasets": [
    {
      "label": "Revenue",
      "data": [120000000, 135000000, 142000000],
      "backgroundColor": "#4CAF50"
    },
    {
      "label": "Expenses", 
      "data": [95000000, 98000000, 110000000],
      "backgroundColor": "#F44336"
    }
  ]
}
```

### 2. Mortality Trend Chart (Line Chart)
```json
{
  "chartType": "line",
  "title": "Mortality Rate Trend",
  "labels": ["Week 1", "Week 2", "Week 3", "Week 4"],
  "datasets": [
    {
      "label": "Mortality Rate (%)",
      "data": [2.1, 1.8, 2.5, 1.9],
      "borderColor": "#FF5722",
      "fill": false
    }
  ]
}
```

### 3. Kandang Utilization Chart (Doughnut Chart)
```json
{
  "chartType": "doughnut",
  "title": "Kandang Utilization",
  "labels": ["Kandang A1", "Kandang A2", "Kandang B1"],
  "datasets": [
    {
      "data": [85, 92, 78],
      "backgroundColor": ["#4CAF50", "#2196F3", "#FF9800"]
    }
  ]
}
```

---

## ?? **Frontend Integration Guide**

### Role-Based Dashboard Loading
```javascript
// Get dashboard based on user role
const getDashboard = async () => {
  const response = await fetch('/api/dashboard', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  return response.json();
};

// Get specific chart data
const getChart = async (chartType, params = {}) => {
  const queryString = new URLSearchParams(params).toString();
  const response = await fetch(`/api/dashboard/charts/${chartType}?${queryString}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  return response.json();
};
```

### Chart.js Integration Example
```javascript
// Revenue vs Expenses Chart
const createRevenueChart = (chartData) => {
  new Chart(ctx, {
    type: chartData.chartType,
    data: {
      labels: chartData.labels,
      datasets: chartData.datasets
    },
    options: {
      responsive: true,
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: (value) => `Rp ${value.toLocaleString()}`
          }
        }
      }
    }
  });
};
```

---

## ?? **Key Business Metrics Calculated**

### Financial Metrics
- **Revenue**: Panen quantity × average weight × price per kg
- **Expenses**: Sum of all Biaya entries
- **Profit Margin**: (Revenue - Expenses) / Revenue × 100
- **Cost per Kg**: Total expenses / total kg produced
- **ROI**: (Net profit / Total investment) × 100

### Operational Metrics
- **Mortality Rate**: Deaths / Total ayams × 100
- **Feed Conversion Ratio**: Feed consumed / Weight gained
- **Kandang Utilization**: Current ayams / Capacity × 100
- **Task Completion Rate**: Completed tasks / Total tasks × 100
- **Productivity Score**: Composite score based on efficiency metrics

### Performance Indicators
- **Health Status**: Based on mortality rates and trends
- **Stock Status**: Critical (<5), Warning (5-10), Good (>10)
- **Alert Severity**: Low, Medium, High, Critical
- **Performance Level**: Excellent, Good, Average, Needs Improvement

---

## ?? **Alert System**

### Stock Alerts
- **Critical**: Pakan ? 5, Vaksin ? 2
- **Warning**: Pakan ? 10, Vaksin ? 5

### Health Alerts  
- **Critical**: Mortality rate > 10%
- **High**: Mortality rate > 5%
- **Medium**: Mortality rate > 2%

### Operational Alerts
- **Overdue Tasks**: Scheduled activities past due time
- **Capacity Alerts**: Kandang utilization > 95%
- **Maintenance**: Equipment/infrastructure needs

---

## ?? **Data Refresh Strategy**

### Real-time Data
- Current stock levels
- Active operations count
- Daily mortality updates

### Hourly Updates
- Task completion status
- Recent operational activities
- Alert notifications

### Daily Aggregation
- Performance calculations
- Trend analysis
- KPI computations

### Weekly/Monthly Reports
- Financial summaries
- Comparative analysis
- Strategic insights generation

---

## ?? **Business Intelligence Features**

### Predictive Analytics (Future Enhancement)
- Mortality rate predictions
- Optimal harvest timing
- Feed consumption forecasting
- Market price predictions

### Comparative Analysis
- Performance vs industry benchmarks
- Month-over-month comparisons
- Year-over-year growth tracking
- Kandang performance rankings

### Strategic Recommendations
- Capacity expansion opportunities
- Cost optimization suggestions
- Efficiency improvement areas
- Risk mitigation strategies

---

## ?? **Dashboard Best Practices Implemented**

1. **Role-Based Access**: Each role sees relevant information
2. **Progressive Disclosure**: Summary ? Details ? Deep dive
3. **Visual Hierarchy**: Most important metrics prominently displayed
4. **Actionable Insights**: Clear next steps and recommendations
5. **Mobile Responsive**: Charts and data adapt to screen size
6. **Performance Optimized**: Efficient data loading and caching
7. **User Experience**: Intuitive navigation and clear labeling

---

## ?? **Ready for Frontend Integration**

Your dashboard system is now complete and ready for frontend integration! The APIs provide:

? **Comprehensive Data**: All metrics needed for business decisions
? **Flexible Charts**: Multiple visualization types with Chart.js compatibility  
? **Role-Based Security**: JWT-protected endpoints with proper authorization
? **Real-time Updates**: Live data refresh capabilities
? **Scalable Architecture**: Easy to extend with new metrics and charts
? **Business Intelligence**: Strategic insights and recommendations

**Next Steps**: 
1. Integrate with your React/Vue/Angular frontend
2. Implement Chart.js or similar charting library
3. Add real-time notifications for critical alerts
4. Customize colors and themes to match your brand