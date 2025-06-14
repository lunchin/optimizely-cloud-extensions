import React, { FunctionComponent } from "react";
import { Link } from "react-router-dom";

const Error: FunctionComponent = () => {
    return (
        <>
            <div id="particles-js" />
            <div className="terminal-window">
                <header>
                    <div className="button green" />
                    <div className="button yellow" />
                    <div className="button red" />
                </header>
                <section className="terminal">
                    <div className="history" />
                    &nbsp;<span className="prompt" />
                    <span className="typed-cursor" />

                </section>
            </div>

            <div className="terminal-data mimik-run-output">
                <br />
                Found 1 feature
                <br />
                ----------------------------------------------
                <br />
                Feature: Bottles
                <span className="gray"># ./features/bottles.feature</span>
                <br />
                <br />

                &nbsp;&nbsp;Scenario: A bottle falls from the wall
                <br />
                &nbsp;&nbsp;&nbsp;&nbsp;<span className="green">✓</span> <span className="gray">Given 100 green bottles are standing</span><br />
                &nbsp;&nbsp;&nbsp;&nbsp;<span className="green">✓</span> <span className="gray">when 1 green bottle accidentally falls</span><br />
                &nbsp;&nbsp;&nbsp;&nbsp;<span className="green">✓</span> <span className="gray">then there are 99 green bottles standing</span><br />
                <br />
                <span className="gray">&nbsp;---------- ----------- ------- -------- --------</span>
                <br />
                &nbsp;&nbsp;Features&nbsp;&nbsp;&nbsp;Scenarios&nbsp;&nbsp;&nbsp;Steps&nbsp;&nbsp;&nbsp;Passed&nbsp;&nbsp;&nbsp;Failed
                <br />
                <span className="gray">&nbsp;---------- ----------- ------- -------- --------</span>
                <br />
                &nbsp;&nbsp;1&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;4
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="green">✓ 4</span>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;0
                <br />
                <br />
                &nbsp;&nbsp;Completed 1 feature in 0.01s<br />
                <br />
            </div>
        </>
    );
}


export default Error;
