import React, { Component } from "react";
import axios from "axios";
import Table from "react-bootstrap/Table";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap/dist/js/bootstrap.bundle.min";
import {Card,Dropdown} from "react-bootstrap";
import "./App.css";

class App extends Component {
	state = {
	  curExchange: "Binance",
	  curAsset: "BTC",
	  curQuote: "USDT",
	  tradeList: [],
	  exchangeList: [],
	  symbolList: [],
	  curAssetChanged: false,
	};
  
	componentDidMount() {
		this.getExchangeList();
		this.getSymbolList();
		this.getTrades();
	}
  
	componentDidUpdate(prevProps, prevState) {
		if (prevState.curExchange !== this.state.curExchange) {
			if(this.state.curExchange.toLowerCase()==="huobi"){
				this.setState({ curAsset: "btc", curQuote: "usdt" });
			}
			else this.setState({ curAsset: "BTC", curQuote: "USDT" });
			this.getSymbolList();
			this.setState({ curAssetChanged: !this.state.curAssetChanged });
		}

		if (
			prevState.curAsset !== this.state.curAsset ||
			prevState.curQuote !== this.state.curQuote ||
			prevState.curAssetChanged !== this.state.curAssetChanged
		){
			this.getTrades();
		}
	}

	getExchangeList = async () => {
		const url = "https://localhost:7279/api/exchangeList";
		try {
			const response = await axios.get(url);
			const exchanges = response.data;
			this.setState({ exchangeList: exchanges });
		} catch (error) {
			alert(error);
		}
	};

	getSymbolList = async () => {
		const url = `https://localhost:7279/api/symbolList/${this.state.curExchange}`;
		try {
			const response = await axios.get(url);
			const symbolListFromApi = response.data;
			this.setState({ symbolList: symbolListFromApi });
		} catch (error) {
			alert(error);
		}
	};

	getTrades = async () => {
		const url = `https://localhost:7279/api/${this.state.curExchange}/${this.state.curAsset}-${this.state.curQuote}`;
		try {
			const response = await axios.get(url);
			const tradeListFromApi = response.data;
			this.setState({ tradeList: tradeListFromApi });
		} catch (error) {
			alert("Give Valid Exchange or Symbol");	
		}
	};

	RowTable = () => (
		<Table striped bordered hover variant="dark" >
			<thead>
			<tr>
				<th scope="col" class="text-center">Asset Symbol</th>
				<th scope="col" class="text-center">Asset Quantity</th>
				<th scope="col" class="text-center">Quote Asset Symbol</th>
				<th scope="col" class="text-center">Quote Asset Quantity</th>
				<th scope="col" class="text-center">Date</th>
			</tr>
			</thead>
			<tbody>
			{this.state.tradeList.map((trade) => (
				<tr>
				<td class="text-center">{trade.assetSymbol}</td>
				<td class="text-center">{trade.assetQuantity}</td>
				<td class="text-center">{trade.quoteAssetSymbol}</td>
				<td class="text-center">{trade.quoteAssetQuantity}</td>
				<td class="text-center">{trade.date}</td>
				</tr>
			))}
			</tbody>
		</Table>
	);

	DropDownExchange = () => (
		<Dropdown className="w-100">
			<Dropdown.Toggle variant="primary" id="dropdown-exchange">
			{this.state.curExchange}
			</Dropdown.Toggle>

			<Dropdown.Menu style={{ maxHeight: "150px", overflowY: "auto" }}>
			{this.state.exchangeList.map((exchange) => (
				<Dropdown.Item
				onClick={() => {
					this.setState({ curExchange: exchange.name });
				}}
				>
				{exchange.name}
				</Dropdown.Item>
			))}
			</Dropdown.Menu>
		</Dropdown>
	);

	DropDownSymbols = () => (
		<Dropdown className="w-100">
			<Dropdown.Toggle variant="primary" id="dropdown-symbols">
			{this.state.curAsset + "-" + this.state.curQuote}
			</Dropdown.Toggle>

			<Dropdown.Menu style={{ maxHeight: "250px", overflowY: "auto" }}>
			{this.state.symbolList.map((symbol) => (
				<Dropdown.Item
				onClick={() => {
					this.setState({curQuote :symbol.Quote});
					this.setState({curAsset :symbol.Base});
				}}
				>
				{symbol.Base + "-" + symbol.Quote}
				</Dropdown.Item>
			))}
			</Dropdown.Menu>
		</Dropdown>
	);

	render(){
		return (
			<div style={{ display: "flex", justifyContent: "center"  }}>
				<Card>
				<Card.Header>
					<div style={{ display: "flex", alignItems: "center" }}>
						<div style={{fontWeight: 'bold', marginRight: "auto" }}>Recent Trades</div>
						<div style={{display: "flex", justifyContent: "flex-end", alignItems: "center",}}>
							{this.DropDownExchange()}
							<div style={{ width: "25px" }} />
							{this.DropDownSymbols()}
						</div>
					</div>
				</Card.Header>
				<Card.Body>
					{this.RowTable()}
				</Card.Body>
				</Card>
			</div>
		)
	}
}

export default App;
